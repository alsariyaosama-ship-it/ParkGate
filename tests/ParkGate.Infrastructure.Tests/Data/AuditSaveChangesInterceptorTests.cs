using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.Data.Interceptors;

namespace ParkGate.Infrastructure.Tests.Data;

public sealed class AuditSaveChangesInterceptorTests
{
    private static readonly DateTime DbNow = new(2026, 10, 5, 15, 30, 0, DateTimeKind.Utc);

    private int _clockReads;

    private AuditSaveChangesInterceptor CreateInterceptor(int? currentUserId) =>
        new(() => currentUserId, (_, _) =>
        {
            _clockReads++;
            return Task.FromResult(DbNow);
        });

    private static ParkGateDbContext CreateContext(AuditSaveChangesInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dummy;Trusted_Connection=True;")
            .AddInterceptors(interceptor)
            .Options;

        return new ParkGateDbContext(options);
    }

    private static DbContextEventData CreateEventData(DbContext context) =>
        new(null!, (_, _) => string.Empty, context);

    private static Organization AttachUnchanged(ParkGateDbContext context, string code, int id)
    {
        var org = new Organization(code, "جهة تجريبية")
        {
            CreatedAtUtc = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 1
        };
        var entry = context.Organizations.Attach(org);
        entry.Property(nameof(Organization.Id)).CurrentValue = id;
        entry.State = EntityState.Unchanged;
        return org;
    }

    [Fact]
    public async Task SavingChangesAsync_OnAddedEntity_ShouldSetCreatedByAndLeaveCreatedAtToTheColumnDefault()
    {
        var interceptor = CreateInterceptor(42);
        using var context = CreateContext(interceptor);
        var org = new Organization("TESTASYNC", "جهة تجريبية");
        context.Organizations.Add(org);

        await interceptor.SavingChangesAsync(CreateEventData(context), default);

        org.CreatedAtUtc.ShouldBe(default);
        org.CreatedByUserId.ShouldBe(42);
        org.UpdatedAtUtc.ShouldBeNull();
        org.UpdatedByUserId.ShouldBeNull();
        _clockReads.ShouldBe(0);
    }

    [Fact]
    public async Task SavingChangesAsync_OnModifiedEntity_ShouldStampUpdatedAtFromTheDatabaseClock()
    {
        var interceptor = CreateInterceptor(99);
        using var context = CreateContext(interceptor);
        var org = AttachUnchanged(context, "TEST", 10);
        org.Update("TEST2", "جهة محدثة", 60);
        context.Entry(org).State = EntityState.Modified;

        await interceptor.SavingChangesAsync(CreateEventData(context), default);

        org.UpdatedAtUtc.ShouldBe(DbNow);
        org.UpdatedByUserId.ShouldBe(99);
        org.CreatedByUserId.ShouldBe(1);
        context.Entry(org).Property(nameof(Organization.CreatedAtUtc)).IsModified.ShouldBeFalse();
    }

    [Fact]
    public async Task SavingChangesAsync_WithSeveralModifiedEntities_ShouldReadTheClockOnce()
    {
        var interceptor = CreateInterceptor(7);
        using var context = CreateContext(interceptor);
        var first = AttachUnchanged(context, "ONE", 11);
        var second = AttachUnchanged(context, "TWO", 12);
        context.Entry(first).State = EntityState.Modified;
        context.Entry(second).State = EntityState.Modified;

        await interceptor.SavingChangesAsync(CreateEventData(context), default);

        _clockReads.ShouldBe(1);
        second.UpdatedAtUtc.ShouldBe(DbNow);
    }

    [Fact]
    public async Task SavingChangesAsync_WithoutAuditableChanges_ShouldNotReadTheClock()
    {
        var interceptor = CreateInterceptor(7);
        using var context = CreateContext(interceptor);
        AttachUnchanged(context, "SAME", 13);

        await interceptor.SavingChangesAsync(CreateEventData(context), default);

        _clockReads.ShouldBe(0);
    }

    [Fact]
    public void SavingChanges_OnModifiedEntity_ShouldStampUpdatedAtFromTheDatabaseClock()
    {
        var interceptor = CreateInterceptor(5);
        using var context = CreateContext(interceptor);
        var org = AttachUnchanged(context, "SYNC", 14);
        context.Entry(org).State = EntityState.Modified;

        interceptor.SavingChanges(CreateEventData(context), default);

        org.UpdatedAtUtc.ShouldBe(DbNow);
        org.UpdatedByUserId.ShouldBe(5);
    }

    [Fact]
    public async Task SavingChangesAsync_WhenCreatedAtUtcAlreadyProvided_ShouldPreserveIt()
    {
        var customCreatedTime = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        var interceptor = CreateInterceptor(null);
        using var context = CreateContext(interceptor);
        var org = new Organization("CUSTOM", "جهة مخصصة") { CreatedAtUtc = customCreatedTime };
        context.Organizations.Add(org);

        await interceptor.SavingChangesAsync(CreateEventData(context), default);

        org.CreatedAtUtc.ShouldBe(customCreatedTime);
        org.CreatedByUserId.ShouldBeNull();
    }

    [Fact]
    public void DynamicContextFactory_ShouldRegisterTheAuditInterceptor()
    {
        var factory = new DynamicParkGateDbContextFactory(new FixedConnection());

        using var context = factory.CreateDbContext();

        context.GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()!
            .Interceptors!.OfType<AuditSaveChangesInterceptor>().ShouldHaveSingleItem();
    }

    private sealed class FixedConnection : ParkGate.Application.Ports.Persistence.IAppConnectionStringProvider
    {
        public string CurrentConnectionString => "Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dummy;Trusted_Connection=True;";

        public void UpdateConnectionString(string connectionString) => throw new NotSupportedException();
    }
}
