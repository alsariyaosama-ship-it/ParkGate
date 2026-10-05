using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.Data.Interceptors;

namespace ParkGate.Infrastructure.Tests.Data;

public sealed class AuditSaveChangesInterceptorTests
{
    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;
        public TestTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;
        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    private static ParkGateDbContext CreateContext(AuditSaveChangesInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dummy;Trusted_Connection=True;")
            .AddInterceptors(interceptor)
            .Options;

        return new ParkGateDbContext(options);
    }

    private static DbContextEventData CreateEventData(DbContext context)
    {
        return new DbContextEventData(null!, (_, _) => string.Empty, context);
    }

    [Fact]
    public void SavingChanges_OnAddedEntity_ShouldSetCreatedAtAndCreatedBy()
    {
        var fixedTime = new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new TestTimeProvider(fixedTime);
        var currentUserId = 42;

        var interceptor = new AuditSaveChangesInterceptor(timeProvider, () => currentUserId);
        using var context = CreateContext(interceptor);

        var org = new Organization("TEST", "جهة تجريبية");
        context.Organizations.Add(org);

        var eventData = CreateEventData(context);
        interceptor.SavingChanges(eventData, default);

        org.CreatedAtUtc.ShouldBe(fixedTime.UtcDateTime);
        org.CreatedByUserId.ShouldBe(currentUserId);
        org.UpdatedAtUtc.ShouldBeNull();
        org.UpdatedByUserId.ShouldBeNull();
    }

    [Fact]
    public async Task SavingChangesAsync_OnAddedEntity_ShouldSetCreatedAtAndCreatedBy()
    {
        var fixedTime = new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new TestTimeProvider(fixedTime);
        var currentUserId = 42;

        var interceptor = new AuditSaveChangesInterceptor(timeProvider, () => currentUserId);
        using var context = CreateContext(interceptor);

        var org = new Organization("TESTASYNC", "جهة تجريبية غير متزامنة");
        context.Organizations.Add(org);

        var eventData = CreateEventData(context);
        await interceptor.SavingChangesAsync(eventData, default);

        org.CreatedAtUtc.ShouldBe(fixedTime.UtcDateTime);
        org.CreatedByUserId.ShouldBe(currentUserId);
        org.UpdatedAtUtc.ShouldBeNull();
        org.UpdatedByUserId.ShouldBeNull();
    }

    [Fact]
    public void SavingChanges_OnModifiedEntity_ShouldSetUpdatedAtAndUpdatedBy()
    {
        var initialTime = new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);
        var updateTime = new DateTimeOffset(2026, 10, 5, 15, 30, 0, TimeSpan.Zero);
        var timeProvider = new TestTimeProvider(updateTime);
        var updaterUserId = 99;

        var interceptor = new AuditSaveChangesInterceptor(timeProvider, () => updaterUserId);
        using var context = CreateContext(interceptor);

        var org = new Organization("TEST", "جهة تجريبية")
        {
            CreatedAtUtc = initialTime.UtcDateTime,
            CreatedByUserId = 1
        };
        var entry = context.Organizations.Attach(org);
        entry.Property(nameof(Organization.Id)).CurrentValue = 10;
        entry.State = EntityState.Unchanged;

        // Modify entity
        org.Update("TEST2", "جهة محدثة", 60);
        entry.State = EntityState.Modified;

        var eventData = CreateEventData(context);
        interceptor.SavingChanges(eventData, default);

        org.CreatedAtUtc.ShouldBe(initialTime.UtcDateTime);
        org.CreatedByUserId.ShouldBe(1);
        org.UpdatedAtUtc.ShouldBe(updateTime.UtcDateTime);
        org.UpdatedByUserId.ShouldBe(updaterUserId);
    }

    [Fact]
    public void SavingChanges_WhenCreatedAtUtcAlreadyProvided_ShouldPreserveIt()
    {
        var customCreatedTime = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        var fixedTime = new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new TestTimeProvider(fixedTime);

        var interceptor = new AuditSaveChangesInterceptor(timeProvider, () => null);
        using var context = CreateContext(interceptor);

        var org = new Organization("CUSTOM", "جهة مخصصة")
        {
            CreatedAtUtc = customCreatedTime
        };
        context.Organizations.Add(org);

        var eventData = CreateEventData(context);
        interceptor.SavingChanges(eventData, default);

        org.CreatedAtUtc.ShouldBe(customCreatedTime);
        org.CreatedByUserId.ShouldBeNull();
    }
}
