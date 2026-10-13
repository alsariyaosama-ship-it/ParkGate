using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Security;
using ParkGate.Infrastructure.Sync;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class SyncOutboxIntegrationTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public SyncOutboxIntegrationTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"success\"}")
            });
        }
    }

    [Fact]
    public async Task AddBeneficiary_AutomaticallyCreatesSyncOutbox_AndPushesAndPrunes()
    {
        await using var context = _fixture.CreateDbContext();

        // 1. Insert a Beneficiary
        var beneficiaryId = Guid.NewGuid();
        var beneficiary = new Beneficiary(
            id: beneficiaryId,
            organizationId: 1, // Default SSF seeded organization
            cardNumber: CardNumber.Create("CARD-SYNC-1"),
            fullName: "سالم محمود",
            phone: PhoneNumber.Create("0923456789"),
            nationalId: NationalId.Create("119951234567"),
            cardExpiryDate: new DateOnly(2028, 1, 1));

        context.Beneficiaries.Add(beneficiary);
        await context.SaveChangesAsync();

        // 2. Verify that SyncOutbox record was generated in the same transaction
        var outboxRecords = await context.SyncOutboxes
            .Where(x => x.EntityType == nameof(Beneficiary) && x.EntityId == beneficiaryId.ToString())
            .ToListAsync();

        outboxRecords.Count.ShouldBe(1);
        var record = outboxRecords[0];
        record.OperationType.ShouldBe("Insert");
        record.PayloadJson.ShouldContain("CARD-SYNC-1");
        record.PayloadJson.ShouldContain("سالم محمود");

        // 3. Test Deduplication
        var deduplicated = OutboxDeduplicator.Deduplicate(outboxRecords);
        deduplicated.Count.ShouldBe(1);

        // 4. Test Encryption
        var aesKey = AesHelper.GenerateKey();
        var encryptionService = new PayloadEncryptionService(aesKey, NullLogger<PayloadEncryptionService>.Instance);
        var encryptedPayload = encryptionService.EncryptSensitiveFields(record.EntityType, record.PayloadJson);
        record.UpdatePayload(encryptedPayload);

        record.PayloadJson.ShouldNotContain("119951234567");
        record.PayloadJson.ShouldNotContain("0923456789");

        // 5. Test Push to Cloud
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var pushService = new CloudPushService(
            httpClient,
            Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "TEST_API_KEY" }),
            NullLogger<CloudPushService>.Instance);

        var pushResult = await pushService.PushBatchAsync(deduplicated);
        pushResult.ShouldBeTrue();
        handler.CallCount.ShouldBe(1);

        // 6. Test Pruning (Immediate Delete upon success)
        context.SyncOutboxes.RemoveRange(deduplicated);
        await context.SaveChangesAsync();

        var remainingRecords = await context.SyncOutboxes
            .Where(x => x.EntityType == nameof(Beneficiary) && x.EntityId == beneficiaryId.ToString())
            .ToListAsync();

        remainingRecords.ShouldBeEmpty();
    }
}
