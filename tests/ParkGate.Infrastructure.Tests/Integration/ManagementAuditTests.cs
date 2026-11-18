using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// Management audit rows are saved in the same unit of work as the change (R6.11), including a create whose
/// audit row names the identity id that only the first save assigns.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ManagementAuditTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public ManagementAuditTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateOrganization_ShouldSaveAuditRowNamingTheNewId()
    {
        var admin = new User($"adm_{Guid.NewGuid():N}"[..10], "المدير", "hash", UserRole.Admin);
        var desk = new Workstation($"WS_{Guid.NewGuid():N}"[..10], "شباك الإدارة");
        await using (var setup = _fixture.CreateDbContext())
        {
            setup.Users.Add(admin);
            setup.Workstations.Add(desk);
            await setup.SaveChangesAsync();
        }

        using var harness = new ServiceHarness(_fixture);
        var code = $"AUD{Guid.NewGuid():N}"[..10].ToUpperInvariant();

        var result = await harness.Organizations(admin, desk).CreateAsync(new CreateOrganizationRequest(code, "جهة التدقيق"));

        result.IsSuccess.ShouldBeTrue();
        await using var context = _fixture.CreateDbContext();
        var row = await context.AuditLogs.AsNoTracking()
            .SingleAsync(a => a.Action == "OrganizationCreated" && a.EntityId == result.Value.Id.ToString());
        row.UserId.ShouldBe(admin.Id);
        row.WorkstationId.ShouldBe(desk.Id);
        result.Value.Id.ShouldBeGreaterThan(0);
    }
}
