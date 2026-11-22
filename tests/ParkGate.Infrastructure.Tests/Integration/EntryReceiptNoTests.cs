using Microsoft.EntityFrameworkCore;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>T10.1, BR-33: the database numbers the entry receipts, and the number stays out of the cloud payload.</summary>
[Trait("Category", "Integration")]
public sealed class EntryReceiptNoTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public EntryReceiptNoTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TwoGrantedEntries_GetIncreasingReceiptNumbers_AndTheNumberIsStored()
    {
        using var harness = new ServiceHarness(_fixture);
        await using var setup = _fixture.CreateDbContext();
        var org = new Organization($"RCP_{Guid.NewGuid():N}"[..10], "جهة الواصل", cooldownMinutes: 720);
        var desk = new Workstation($"RCD_{Guid.NewGuid():N}"[..10], "شباك الواصل");
        var user = new User($"rcu_{Guid.NewGuid():N}"[..10], "مشغل", "hash", UserRole.Operator);
        setup.Organizations.Add(org);
        setup.Workstations.Add(desk);
        setup.Users.Add(user);
        await setup.SaveChangesAsync();

        var first = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create($"RCP-{Guid.NewGuid():N}"[..12]), "حامل الواصل 1", PhoneNumber.Create("0915550001"));
        var second = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create($"RCP-{Guid.NewGuid():N}"[..12]), "حامل الواصل 2", PhoneNumber.Create("0915550002"));
        setup.Beneficiaries.AddRange(first, second);
        await setup.SaveChangesAsync();

        var service = harness.CheckIn(user, desk);
        var one = await service.RecordEntryAsync(new RecordEntryRequest(first.Id));
        var two = await service.RecordEntryAsync(new RecordEntryRequest(second.Id));

        one.IsSuccess.ShouldBeTrue();
        two.IsSuccess.ShouldBeTrue();
        one.Value.ReceiptNo.ShouldBeGreaterThan(0);
        two.Value.ReceiptNo.ShouldBeGreaterThan(one.Value.ReceiptNo);

        await using var verify = _fixture.CreateDbContext();
        (await verify.EntryAttempts.SingleAsync(a => a.Id == one.Value.AttemptId)).ReceiptNo.ShouldBe(one.Value.ReceiptNo);
        (await verify.EntryAttempts.SingleAsync(a => a.Id == two.Value.AttemptId)).ReceiptNo.ShouldBe(two.Value.ReceiptNo);
    }

    [Fact]
    public async Task EntryAttemptOutboxPayload_DoesNotContainTheReceiptNumber()
    {
        using var harness = new ServiceHarness(_fixture);
        await using var setup = _fixture.CreateDbContext();
        var org = new Organization($"RCO_{Guid.NewGuid():N}"[..10], "جهة الواصل والمزامنة", cooldownMinutes: 720);
        var desk = new Workstation($"RCW_{Guid.NewGuid():N}"[..10], "شباك المزامنة");
        var user = new User($"rco_{Guid.NewGuid():N}"[..10], "مشغل", "hash", UserRole.Operator);
        setup.Organizations.Add(org);
        setup.Workstations.Add(desk);
        setup.Users.Add(user);
        await setup.SaveChangesAsync();
        var holder = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create($"RCO-{Guid.NewGuid():N}"[..12]), "حامل المزامنة", PhoneNumber.Create("0915550003"));
        setup.Beneficiaries.Add(holder);
        await setup.SaveChangesAsync();

        var result = await harness.CheckIn(user, desk).RecordEntryAsync(new RecordEntryRequest(holder.Id));
        result.IsSuccess.ShouldBeTrue();

        await using var verify = _fixture.CreateDbContext();
        var outbox = await verify.SyncOutboxes
            .Where(x => x.EntityType == nameof(EntryAttempt) && x.EntityId == result.Value.AttemptId.ToString())
            .ToListAsync();

        outbox.ShouldNotBeEmpty();
        outbox.ShouldAllBe(x => x.PayloadJson.Contains("\"TotalPersons\"") && !x.PayloadJson.Contains("ReceiptNo"));
    }
}
