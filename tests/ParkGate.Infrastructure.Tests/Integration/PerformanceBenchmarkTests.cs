using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.Repositories;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class PerformanceBenchmarkTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;
    private readonly IEntryEligibilityPolicy _policy = new RollingWindowEligibilityPolicy();

    public PerformanceBenchmarkTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CardLookupAndEligibility_UnderHighVolumeDataset_CompletesInLessThanOneSecond()
    {
        // 1. Setup seed environment using EF entities
        await using var context = _fixture.CreateDbContext();
        context.Database.SetCommandTimeout(300);

        var org = new Organization($"PERF_{Guid.NewGuid():N}"[..8], "جهة اختبار الأداء", cooldownMinutes: 2880);
        var ws = new Workstation($"WS_{Guid.NewGuid():N}"[..8], "محطة اختبار الأداء");
        var user = new User($"op_{Guid.NewGuid():N}"[..8], "مشغل الأداء", "dummy_hash", UserRole.Operator);

        context.Organizations.Add(org);
        context.Workstations.Add(ws);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var orgId = org.Id;
        var wsId = ws.Id;
        var userId = user.Id;

        // 2. Generate 100,000 Holders using fast T-SQL set generation
        // Tally table pattern produces 100,000 rows in ~1-2 seconds
        var generateHoldersSql = $@"
            WITH
            L0 AS (SELECT 1 AS c FROM (VALUES(1),(1),(1),(1),(1),(1),(1),(1),(1),(1)) AS D(c)),
            L1 AS (SELECT 1 AS c FROM L0 AS A CROSS JOIN L0 AS B),
            L2 AS (SELECT 1 AS c FROM L1 AS A CROSS JOIN L1 AS B),
            L3 AS (SELECT 1 AS c FROM L2 AS A CROSS JOIN L0 AS B),
            Nums AS (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS n FROM L3)
            INSERT INTO Beneficiaries WITH (TABLOCK) (
                Id, OrganizationId, CardNumber, FullName, Phone, NationalId,
                Status, StatusReason, Notes, LastGrantedEntryUtc, CreatedAtUtc
            )
            SELECT 
                NEWID(),
                {orgId},
                'CARD-' + RIGHT('00000000' + CAST(n AS VARCHAR(10)), 8),
                N'مستفيد تجريبي رقم ' + CAST(n AS NVARCHAR(10)),
                '091' + RIGHT('0000000' + CAST(n AS VARCHAR(10)), 7),
                RIGHT('119900000000' + CAST(n AS VARCHAR(12)), 12),
                1, -- Active
                NULL,
                NULL,
                DATEADD(hour, - (n % 100), SYSUTCDATETIME()),
                SYSUTCDATETIME()
            FROM Nums;
        ";
        await context.Database.ExecuteSqlRawAsync(generateHoldersSql);

        var holderCount = await context.Beneficiaries.CountAsync();
        holderCount.ShouldBeGreaterThanOrEqualTo(100_000);

        // 3. Generate 1,000,000 attempts using set cross join
        // 10 attempts for each of the 100,000 holders = 1,000,000 rows
        var generateAttemptsFastSql = $@"
            WITH
            Att10 AS (SELECT n FROM (VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) AS D(n))
            INSERT INTO EntryAttempts WITH (TABLOCK) (
                Id, BeneficiaryId, OrganizationId, AttemptedAtUtc, BusinessDate,
                Result, DenialReason, CooldownMinutesApplied, NextEligibleAtUtc,
                TotalPersons, Channel, WorkstationId, UserId,
                OverrideByUserId, OverrideReason, IsVoided, VoidedAtUtc, VoidedByUserId, VoidReason
            )
            SELECT
                NEWID(),
                b.Id,
                {orgId},
                DATEADD(hour, - (att.n * 72), SYSUTCDATETIME()),
                CAST(DATEADD(hour, - (att.n * 72), SYSUTCDATETIME()) AS DATE),
                1, -- Granted
                NULL,
                2880,
                NULL,
                1 + (att.n % 4),
                1, -- StandardDesk
                {wsId},
                {userId},
                NULL,
                NULL,
                0,
                NULL,
                NULL,
                NULL
            FROM Beneficiaries b WITH (NOLOCK)
            CROSS JOIN Att10 att;
        ";
        await context.Database.ExecuteSqlRawAsync(generateAttemptsFastSql);

        var attemptCount = await context.EntryAttempts.CountAsync();
        attemptCount.ShouldBeGreaterThanOrEqualTo(1_000_000);

        // 4. Test Performance Requirement (T8.5): Lookup < 1 s (1000 ms)
        var beneficiaryRepo = new BeneficiaryRepository(context);
        var orgRepo = new OrganizationRepository(context);

        // Warm-up query compilation (EF Core LINQ translation and connection pool warming)
        var warmupCard = CardNumber.Create("CARD-00000001");
        _ = await beneficiaryRepo.GetByCardNumberAsync(warmupCard);
        _ = await orgRepo.GetByIdAsync(orgId);
        _ = await beneficiaryRepo.GetLatestGrantedEntryUtcAsync(Guid.NewGuid());

        var testCards = new[] { "CARD-00000001", "CARD-00050000", "CARD-00100000" };

        foreach (var cardStr in testCards)
        {
            var card = CardNumber.Create(cardStr);

            var stopwatch = Stopwatch.StartNew();

            // A) Lookup Beneficiary by CardNumber (IX_Beneficiaries_CardNumber seek)
            var beneficiaries = await beneficiaryRepo.GetByCardNumberAsync(card);
            beneficiaries.Count.ShouldBeGreaterThan(0);
            var beneficiary = beneficiaries[0];

            // B) Lookup Organization
            var currentOrg = await orgRepo.GetByIdAsync(beneficiary.OrganizationId);
            currentOrg.ShouldNotBeNull();

            // C) Lookup Latest Entry Attempt (IX_EntryAttempts_Beneficiary_Time seek)
            var latestEntryUtc = await beneficiaryRepo.GetLatestGrantedEntryUtcAsync(beneficiary.Id);

            // D) Evaluate Policy in Domain
            var contextDecision = new EligibilityContext(
                currentOrg,
                beneficiary,
                defaultCooldownMinutes: 2880,
                DateTime.UtcNow,
                DateOnly.FromDateTime(DateTime.UtcNow));

            var decision = _policy.Evaluate(contextDecision);
            decision.ShouldNotBeNull();

            stopwatch.Stop();

            // Benchmark Assertion: T8.5 specifies lookup < 1 s (1000 ms)
            // With indexed seeks, execution is well under 100 ms
            stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000);
        }
    }
}
