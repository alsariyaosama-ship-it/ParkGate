using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data;

public class ParkGateDbContext : DbContext
{
    public ParkGateDbContext(DbContextOptions<ParkGateDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<AgeCategory> AgeCategories => Set<AgeCategory>();
    public DbSet<EntryAttempt> EntryAttempts => Set<EntryAttempt>();
    public DbSet<EntryAttemptCompanion> EntryAttemptCompanions => Set<EntryAttemptCompanion>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Workstation> Workstations => Set<Workstation>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParkGateDbContext).Assembly);
        modelBuilder.SeedInitialData();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTime>()
            .HavePrecision(3)
            .HaveConversion<DateTimeUtcConverter>();

        configurationBuilder.Properties<DateTime?>()
            .HavePrecision(3)
            .HaveConversion<NullableDateTimeUtcConverter>();

        configurationBuilder.Properties<DateOnly>()
            .HaveColumnType("date");
    }
}
