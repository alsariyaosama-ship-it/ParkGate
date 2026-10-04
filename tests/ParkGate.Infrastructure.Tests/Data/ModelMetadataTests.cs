using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Data;

public sealed class ModelMetadataTests
{
    private static IModel GetDesignModel(out ParkGateDbContext context)
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ParkGate_MetadataTest;Trusted_Connection=True;")
            .Options;

        context = new ParkGateDbContext(options);
        return context.GetService<IDesignTimeModel>().Model;
    }

    [Fact]
    public void Organizations_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(Organization));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("Organizations");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(Organization.Id));

            var codeProp = entity.FindProperty(nameof(Organization.Code))!;
            codeProp.GetMaxLength().ShouldBe(Organization.MaxCodeLength);
            codeProp.IsNullable.ShouldBeFalse();

            var codeIndex = entity.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(Organization.Code)));
            codeIndex.ShouldNotBeNull();
            codeIndex.IsUnique.ShouldBeTrue();
            codeIndex.GetDatabaseName().ShouldBe("UX_Organizations_Code");

            var nameProp = entity.FindProperty(nameof(Organization.NameAr))!;
            nameProp.GetMaxLength().ShouldBe(Organization.MaxNameLength);
            nameProp.IsNullable.ShouldBeFalse();

            var isDefaultIndex = entity.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(Organization.IsDefault)));
            isDefaultIndex.ShouldNotBeNull();
            isDefaultIndex.IsUnique.ShouldBeTrue();
            isDefaultIndex.GetFilter().ShouldBe("[IsDefault] = 1");
            isDefaultIndex.GetDatabaseName().ShouldBe("UX_Organizations_IsDefault");

            var checkConstraints = entity.GetCheckConstraints();
            checkConstraints.ShouldContain(c => c.Name == "CK_Organizations_CooldownMinutes");

            var rowVersion = entity.FindProperty(nameof(Organization.RowVersion))!;
            rowVersion.IsConcurrencyToken.ShouldBeTrue();
        }
    }

    [Fact]
    public void Beneficiaries_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(Beneficiary));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("Beneficiaries");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(Beneficiary.Id));

            var cardProp = entity.FindProperty(nameof(Beneficiary.CardNumber))!;
            cardProp.GetMaxLength().ShouldBe(CardNumber.MaxLength);
            cardProp.IsNullable.ShouldBeFalse();

            var phoneProp = entity.FindProperty(nameof(Beneficiary.Phone))!;
            phoneProp.GetMaxLength().ShouldBe(15);
            phoneProp.IsNullable.ShouldBeFalse();

            var nationalIdProp = entity.FindProperty(nameof(Beneficiary.NationalId))!;
            nationalIdProp.GetMaxLength().ShouldBe(NationalId.RequiredLength);
            nationalIdProp.IsNullable.ShouldBeTrue();

            var nationalIdIndex = entity.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(Beneficiary.NationalId)));
            nationalIdIndex.ShouldNotBeNull();
            nationalIdIndex.IsUnique.ShouldBeTrue();
            nationalIdIndex.GetFilter().ShouldBe("[NationalId] IS NOT NULL");
            nationalIdIndex.GetDatabaseName().ShouldBe("UX_Beneficiaries_NationalId");

            var orgCardIndex = entity.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == "UX_Beneficiaries_Org_Card");
            orgCardIndex.ShouldNotBeNull();
            orgCardIndex.IsUnique.ShouldBeTrue();
            orgCardIndex.Properties.Select(p => p.Name).ShouldBe([nameof(Beneficiary.OrganizationId), nameof(Beneficiary.CardNumber)]);

            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_Beneficiaries_CardNumber");
            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_Beneficiaries_Phone");
            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_Beneficiaries_FullName");

            var statusReasonConstraint = entity.GetCheckConstraints().SingleOrDefault(c => c.Name == "CK_Beneficiaries_StatusReason");
            statusReasonConstraint.ShouldNotBeNull();

            var rowVersion = entity.FindProperty(nameof(Beneficiary.RowVersion))!;
            rowVersion.IsConcurrencyToken.ShouldBeTrue();
        }
    }

    [Fact]
    public void AgeCategories_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(AgeCategory));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("AgeCategories");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(AgeCategory.Id));

            var nameProp = entity.FindProperty(nameof(AgeCategory.NameAr))!;
            nameProp.GetMaxLength().ShouldBe(AgeCategory.MaxNameLength);
            nameProp.IsNullable.ShouldBeFalse();

            var nameIndex = entity.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == "UX_AgeCategories_NameAr");
            nameIndex.ShouldNotBeNull();
            nameIndex.IsUnique.ShouldBeTrue();

            var descProp = entity.FindProperty(nameof(AgeCategory.DescriptionAr))!;
            descProp.GetMaxLength().ShouldBe(AgeCategory.MaxDescriptionLength);
            descProp.IsNullable.ShouldBeTrue();
        }
    }

    [Fact]
    public void EntryAttempts_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(EntryAttempt));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("EntryAttempts");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(EntryAttempt.Id));

            var businessDateProp = entity.FindProperty(nameof(EntryAttempt.BusinessDate))!;
            businessDateProp.GetColumnType().ShouldBe("date");

            var checkConstraints = entity.GetCheckConstraints();
            checkConstraints.ShouldContain(c => c.Name == "CK_EntryAttempts_DenialReason");
            checkConstraints.ShouldContain(c => c.Name == "CK_EntryAttempts_Override");
            checkConstraints.ShouldContain(c => c.Name == "CK_EntryAttempts_Void");

            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_EntryAttempts_Beneficiary_Time");
            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_EntryAttempts_BusinessDate");
            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_EntryAttempts_Workstation_Time");

            var companionsNav = entity.FindNavigation(nameof(EntryAttempt.Companions));
            companionsNav.ShouldNotBeNull();

            var rowVersion = entity.FindProperty(nameof(EntryAttempt.RowVersion))!;
            rowVersion.IsConcurrencyToken.ShouldBeTrue();
        }
    }

    [Fact]
    public void EntryAttemptCompanions_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(EntryAttemptCompanion));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("EntryAttemptCompanions");
            var pk = entity.FindPrimaryKey()!;
            pk.Properties.Select(p => p.Name).ShouldBe([nameof(EntryAttemptCompanion.EntryAttemptId), nameof(EntryAttemptCompanion.AgeCategoryId)]);

            var checkConstraints = entity.GetCheckConstraints();
            checkConstraints.ShouldContain(c => c.Name == "CK_EntryAttemptCompanions_Count");
        }
    }

    [Fact]
    public void Users_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(User));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("Users");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(User.Id));

            var userProp = entity.FindProperty(nameof(User.UserName))!;
            userProp.GetMaxLength().ShouldBe(User.MaxUserNameLength);
            userProp.IsNullable.ShouldBeFalse();

            var userNameIndex = entity.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == "UX_Users_UserName");
            userNameIndex.ShouldNotBeNull();
            userNameIndex.IsUnique.ShouldBeTrue();

            var passProp = entity.FindProperty(nameof(User.PasswordHash))!;
            passProp.GetMaxLength().ShouldBe(User.MaxPasswordHashLength);

            var rowVersion = entity.FindProperty(nameof(User.RowVersion))!;
            rowVersion.IsConcurrencyToken.ShouldBeTrue();
        }
    }

    [Fact]
    public void Workstations_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(Workstation));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("Workstations");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(Workstation.Id));

            var machineProp = entity.FindProperty(nameof(Workstation.MachineName))!;
            machineProp.GetMaxLength().ShouldBe(Workstation.MaxMachineNameLength);

            var machineIndex = entity.GetIndexes().SingleOrDefault(i => i.GetDatabaseName() == "UX_Workstations_MachineName");
            machineIndex.ShouldNotBeNull();
            machineIndex.IsUnique.ShouldBeTrue();
        }
    }

    [Fact]
    public void AppSettings_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(AppSetting));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("AppSettings");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(AppSetting.Key));

            var keyProp = entity.FindProperty(nameof(AppSetting.Key))!;
            keyProp.GetMaxLength().ShouldBe(AppSetting.MaxKeyLength);

            var valProp = entity.FindProperty(nameof(AppSetting.Value))!;
            valProp.GetMaxLength().ShouldBe(AppSetting.MaxValueLength);
        }
    }

    [Fact]
    public void AuditLogs_Metadata_ShouldMatchDatabaseDesign()
    {
        var model = GetDesignModel(out var context);
        using (context)
        {
            var entity = model.FindEntityType(typeof(AuditLog));
            entity.ShouldNotBeNull();

            entity.GetTableName().ShouldBe("AuditLogs");
            entity.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(AuditLog.Id));

            var actionProp = entity.FindProperty(nameof(AuditLog.Action))!;
            actionProp.GetMaxLength().ShouldBe(AuditLog.MaxActionLength);

            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_AuditLogs_AtUtc");
            entity.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_AuditLogs_Entity");
        }
    }
}
