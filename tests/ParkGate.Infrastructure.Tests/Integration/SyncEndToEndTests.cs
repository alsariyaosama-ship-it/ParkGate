using System.Net;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Sync;
using ParkGate.SyncAgent.Workers;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// R2 "Done when": holders, entries and gift vouchers go through the real outbox, the real worker and the real
/// push service to a cloud that first drops the connection and then answers 503, and the cloud ends up with the
/// local data minus the fields that must stay in the park.
/// </summary>
[Trait("Category", "Integration")]
public sealed class SyncEndToEndTests : IClassFixture<LocalDbTestFixture>
{
    private const string Phone = "0912345678";
    private const string NationalIdValue = "119900123456";

    private readonly LocalDbTestFixture _fixture;

    public SyncEndToEndTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HoldersEntriesAndVouchers_ThroughFailingThenRecoveringCloud_ArriveWithoutSensitiveFields()
    {
        var seed = await SeedAsync();
        var voucherToken = seed.Voucher.Token.ToString();

        // Connection drops, then the server errors for one batch plus three single-record retries, then it recovers.
        var cloud = new FakeCloud(
            FakeCloud.Unreachable,
            FakeCloud.ServerError, FakeCloud.ServerError, FakeCloud.ServerError, FakeCloud.ServerError);
        var worker = CreateWorker(cloud);
        var options = new SyncOptions { Enabled = true, BatchSize = 500, MaxRetries = 3 };

        var pendingBefore = await CountPendingAsync();
        pendingBefore.ShouldBeGreaterThan(0);

        await worker.ProcessBatchAsync(options, CancellationToken.None);
        (await CountPendingAsync()).ShouldBe(pendingBefore, "an unreachable cloud must not lose or consume anything");
        (await MaxRetryCountAsync()).ShouldBe(0, "a network outage is not the record's fault");
        cloud.Stored.ShouldBeEmpty();

        await worker.ProcessBatchAsync(options, CancellationToken.None);
        (await CountPendingAsync()).ShouldBe(pendingBefore, "server errors must keep every record queued");
        (await MaxRetryCountAsync()).ShouldBeLessThan(options.MaxRetries, "the records must stay eligible for the next cycle");
        cloud.Stored.ShouldBeEmpty();

        await worker.ProcessBatchAsync(options, CancellationToken.None);
        (await CountPendingAsync()).ShouldBe(0, "a recovered cloud takes the whole backlog");

        AssertCloudMatchesLocal(cloud, seed);
        AssertNothingSensitiveWasSent(cloud, voucherToken);
    }

    private static void AssertCloudMatchesLocal(FakeCloud cloud, Seed seed)
    {
        var organization = cloud.Single("Organization", seed.Organization.Id.ToString());
        organization["Code"]!.GetValue<string>().ShouldBe("E2E");
        organization["GiftEnabled"]!.GetValue<bool>().ShouldBeTrue();

        var category = cloud.Single("AgeCategory", seed.Category.Id.ToString());
        category["NameAr"]!.GetValue<string>().ShouldBe(seed.Category.NameAr);

        var holder = cloud.Single("Beneficiary", seed.Holder.Id.ToString());
        holder["FullName"]!.GetValue<string>().ShouldBe(seed.Holder.FullName);
        holder["OrganizationId"]!.GetValue<int>().ShouldBe(seed.Organization.Id);
        holder.ToJsonString().ShouldContain("E2E-CARD-1");

        var attempt = cloud.Single("EntryAttempt", seed.Attempt.Id.ToString());
        attempt["TotalPersons"]!.GetValue<int>().ShouldBe(seed.Attempt.TotalPersons);
        attempt["BeneficiaryId"]!.GetValue<Guid>().ShouldBe(seed.Holder.Id);
        attempt["BusinessDate"]!.GetValue<string>().ShouldBe("2026-11-13");

        cloud.Stored.Keys.ShouldContain(k => k.EntityType == "EntryAttemptCompanion");

        var voucher = cloud.Single("GiftVoucher", seed.Voucher.Id.ToString());
        voucher["Value"]!.GetValue<decimal>().ShouldBe(seed.Voucher.Value);
        voucher["BeneficiaryId"]!.GetValue<Guid>().ShouldBe(seed.Holder.Id);
    }

    private static void AssertNothingSensitiveWasSent(FakeCloud cloud, string voucherToken)
    {
        cloud.Requests.Count.ShouldBeGreaterThanOrEqualTo(6, "the failed attempts were requests too and are checked as well");

        foreach (var body in cloud.Requests)
        {
            body.ShouldNotContain(Phone);
            body.ShouldNotContain(NationalIdValue);
            body.ShouldNotContain(voucherToken, Case.Insensitive);
            body.ShouldNotContain("\\\"Phone\\\"");
            body.ShouldNotContain("\\\"NationalId\\\"");
            body.ShouldNotContain("\\\"Token\\\"");
            body.ShouldNotContain("\\\"KeyId\\\"");
        }
    }

    private OutboxProcessorWorker CreateWorker(FakeCloud cloud)
    {
        var push = new CloudPushService(
            new HttpClient(cloud),
            Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "E2E-KEY" }),
            NullLogger<CloudPushService>.Instance);

        var monitor = Substitute.For<IOptionsMonitor<SyncOptions>>();
        monitor.CurrentValue.Returns(new SyncOptions { Enabled = true, BatchSize = 500, MaxRetries = 3 });

        return new OutboxProcessorWorker(new FixtureUnitOfWorkFactory(_fixture), push, monitor, NullLogger<OutboxProcessorWorker>.Instance);
    }

    private async Task<int> CountPendingAsync()
    {
        await using var context = _fixture.CreateDbContext();
        return await context.SyncOutboxes.CountAsync();
    }

    private async Task<int> MaxRetryCountAsync()
    {
        await using var context = _fixture.CreateDbContext();
        return await context.SyncOutboxes.MaxAsync(x => (int?)x.RetryCount) ?? 0;
    }

    private async Task<Seed> SeedAsync()
    {
        await using var context = _fixture.CreateDbContext();

        var organization = new Organization("E2E", "جهة الاختبار الشامل", cooldownMinutes: 120, isDefault: false, isActive: true);
        organization.ConfigureGift(true, 12.345m, 30);
        var user = new User("e2e_user", "مشغل", "PBKDF2$SHA256$600000$c2FsdA==$aGFzaA==", UserRole.Operator, false, true);
        var workstation = new Workstation("E2E-PC", "شباك الاختبار", WorkstationType.Desk, isActive: true);
        var category = new AgeCategory("مرافق", "اختبار", 10, isActive: true);
        context.AddRange(organization, user, workstation, category);
        await context.SaveChangesAsync();

        var holder = new Beneficiary(
            Guid.NewGuid(),
            organization.Id,
            CardNumber.Create("E2E-CARD-1"),
            "سالم محمود الورفلي",
            PhoneNumber.Create(Phone),
            NationalId.Create(NationalIdValue),
            new DateOnly(2028, 1, 1));
        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();

        var attemptedAt = new DateTime(2026, 11, 13, 10, 0, 0, DateTimeKind.Utc);
        var attempt = EntryAttempt.CreateAllowed(
            Guid.NewGuid(),
            holder.Id,
            organization.Id,
            attemptedAt,
            new DateOnly(2026, 11, 13),
            120,
            attemptedAt.AddMinutes(120),
            workstation.Id,
            user.Id,
            EntryChannel.Desk,
            [new EntryAttemptCompanion(category.Id, 2)]);
        context.EntryAttempts.Add(attempt);

        var voucher = GiftVoucher.Issue(
            Guid.NewGuid(),
            1,
            holder.Id,
            organization.Id,
            12.345m,
            attemptedAt,
            new DateOnly(2026, 11, 13),
            user.Id,
            workstation.Id);
        context.GiftVouchers.Add(voucher);
        await context.SaveChangesAsync();

        return new Seed(organization, category, holder, attempt, voucher);
    }

    private sealed record Seed(Organization Organization, AgeCategory Category, Beneficiary Holder, EntryAttempt Attempt, GiftVoucher Voucher);

    private sealed class FixtureUnitOfWorkFactory(LocalDbTestFixture fixture) : IUnitOfWorkFactory
    {
        public IUnitOfWork Create() => fixture.CreateUnitOfWork();
    }

    /// <summary>
    /// An in-process stand-in for the web API: it plays a script of failures first, then accepts every batch
    /// and keeps the last payload per entity, which is what the cloud database would hold.
    /// </summary>
    private sealed class FakeCloud : HttpMessageHandler
    {
        public static readonly Func<HttpResponseMessage> Unreachable = () => throw new HttpRequestException("connection refused");

        public static readonly Func<HttpResponseMessage> ServerError = () => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        private readonly Queue<Func<HttpResponseMessage>> _script;

        public FakeCloud(params Func<HttpResponseMessage>[] script)
        {
            _script = new Queue<Func<HttpResponseMessage>>(script);
        }

        public List<string> Requests { get; } = [];

        public Dictionary<(string EntityType, string EntityId), JsonObject> Stored { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            Requests.Add(body);

            if (_script.TryDequeue(out var scripted))
            {
                return scripted();
            }

            foreach (var record in JsonNode.Parse(body)!["records"]!.AsArray())
            {
                var entityType = record!["entityType"]!.GetValue<string>();
                var entityId = record["entityId"]!.GetValue<string>();
                var payload = JsonNode.Parse(record["payloadJson"]!.GetValue<string>())!.AsObject();
                Stored[(entityType, entityId)] = payload;
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"success\":true}") };
        }

        public JsonObject Single(string entityType, string entityId)
        {
            Stored.TryGetValue((entityType, entityId), out var payload).ShouldBeTrue($"{entityType} {entityId} never reached the cloud");
            return payload!;
        }
    }
}
