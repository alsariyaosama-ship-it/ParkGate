using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Data;

public sealed class UnitOfWorkTests
{
    private static ParkGateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dummy;Trusted_Connection=True;")
            .Options;

        return new ParkGateDbContext(options);
    }

    [Fact]
    public void UnitOfWork_ShouldInstantiateAllRepositories()
    {
        using var context = CreateContext();
        using var uow = new UnitOfWork(context);

        uow.Beneficiaries.ShouldNotBeNull();
        uow.Organizations.ShouldNotBeNull();
        uow.EntryAttempts.ShouldNotBeNull();
        uow.AgeCategories.ShouldNotBeNull();
        uow.Users.ShouldNotBeNull();
        uow.Workstations.ShouldNotBeNull();
        uow.AppSettings.ShouldNotBeNull();
        uow.AuditLogs.ShouldNotBeNull();
    }

    [Fact]
    public async Task CommitAsync_WithoutActiveTransaction_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        using var uow = new UnitOfWork(context);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await uow.CommitAsync();
        });
    }

    [Fact]
    public async Task RollbackAsync_WithoutActiveTransaction_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        using var uow = new UnitOfWork(context);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await uow.RollbackAsync();
        });
    }

    [Fact]
    public void UnitOfWorkFactory_Create_ShouldReturnNewUnitOfWork()
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<ParkGateDbContext>>();
        dbContextFactory.CreateDbContext().Returns(_ => CreateContext());

        var factory = new UnitOfWorkFactory(dbContextFactory);
        var uow = factory.Create();

        uow.ShouldNotBeNull();
        uow.Beneficiaries.ShouldNotBeNull();
    }
}
