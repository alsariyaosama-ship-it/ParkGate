using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data.Interceptors;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>Audit columns take the database clock, not the desk clock (BR-05, R6.3).</summary>
[Trait("Category", "Integration")]
public sealed class AuditColumnsTests : IClassFixture<LocalDbTestFixture>
{
    private const int DeskUserId = 77;
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(5);

    private readonly LocalDbTestFixture _fixture;

    public AuditColumnsTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InsertThenUpdate_ShouldStampCreatedAndUpdatedColumnsFromTheDatabase()
    {
        var interceptor = new AuditSaveChangesInterceptor(() => DeskUserId);
        var code = $"AC{Guid.NewGuid():N}"[..10].ToUpperInvariant();

        await using (var insert = _fixture.CreateDbContext(interceptor))
        {
            insert.Organizations.Add(new Organization(code, "جهة الأعمدة"));
            await insert.SaveChangesAsync();
        }

        await using (var update = _fixture.CreateDbContext(interceptor))
        {
            var org = await update.Organizations.SingleAsync(o => o.Code == code);
            org.Update(code, "جهة الأعمدة المعدلة", 60);
            await update.SaveChangesAsync();
        }

        await using var uow = _fixture.CreateUnitOfWork();
        var dbNow = await uow.GetDbUtcNowAsync();
        await using var read = _fixture.CreateDbContext();
        var saved = await read.Organizations.AsNoTracking().SingleAsync(o => o.Code == code);

        saved.CreatedByUserId.ShouldBe(DeskUserId);
        (dbNow - saved.CreatedAtUtc).Duration().ShouldBeLessThan(Tolerance);
        saved.UpdatedByUserId.ShouldBe(DeskUserId);
        saved.UpdatedAtUtc.ShouldNotBeNull();
        (dbNow - saved.UpdatedAtUtc!.Value).Duration().ShouldBeLessThan(Tolerance);
    }
}
