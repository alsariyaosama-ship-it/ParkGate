using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgeCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgeCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    WorkstationId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CooldownMinutes = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.CheckConstraint("CK_Organizations_CooldownMinutes", "[CooldownMinutes] IS NULL OR ([CooldownMinutes] >= 1 AND [CooldownMinutes] <= 10080)");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Role = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockoutUntilUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    LastLoginUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workstations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workstations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Beneficiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    CardExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    StatusReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastGrantedEntryUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiaries", x => x.Id);
                    table.CheckConstraint("CK_Beneficiaries_StatusReason", "([Status] = 1) OR ([Status] <> 1 AND [StatusReason] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Beneficiaries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntryAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BeneficiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    AttemptedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Result = table.Column<byte>(type: "tinyint", nullable: false),
                    DenialReason = table.Column<byte>(type: "tinyint", nullable: true),
                    CooldownMinutesApplied = table.Column<int>(type: "int", nullable: false),
                    NextEligibleAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    TotalPersons = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<byte>(type: "tinyint", nullable: false),
                    WorkstationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OverrideByUserId = table.Column<int>(type: "int", nullable: true),
                    OverrideReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsVoided = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VoidedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    VoidedByUserId = table.Column<int>(type: "int", nullable: true),
                    VoidReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryAttempts", x => x.Id);
                    table.CheckConstraint("CK_EntryAttempts_DenialReason", "([Result] = 2 AND [DenialReason] IS NOT NULL) OR ([Result] <> 2 AND [DenialReason] IS NULL)");
                    table.CheckConstraint("CK_EntryAttempts_Override", "([Result] = 3 AND [OverrideReason] IS NOT NULL AND [OverrideByUserId] IS NOT NULL) OR ([Result] <> 3 AND [OverrideReason] IS NULL AND [OverrideByUserId] IS NULL)");
                    table.CheckConstraint("CK_EntryAttempts_Void", "([IsVoided] = 0 AND [VoidedAtUtc] IS NULL AND [VoidedByUserId] IS NULL AND [VoidReason] IS NULL) OR ([IsVoided] = 1 AND [VoidedAtUtc] IS NOT NULL AND [VoidedByUserId] IS NOT NULL AND [VoidReason] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_EntryAttempts_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttempts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttempts_Users_OverrideByUserId",
                        column: x => x.OverrideByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttempts_Users_VoidedByUserId",
                        column: x => x.VoidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttempts_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntryAttemptCompanions",
                columns: table => new
                {
                    EntryAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgeCategoryId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryAttemptCompanions", x => new { x.EntryAttemptId, x.AgeCategoryId });
                    table.CheckConstraint("CK_EntryAttemptCompanions_Count", "[Count] >= 0");
                    table.ForeignKey(
                        name: "FK_EntryAttemptCompanions_AgeCategories_AgeCategoryId",
                        column: x => x.AgeCategoryId,
                        principalTable: "AgeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttemptCompanions_EntryAttempts_EntryAttemptId",
                        column: x => x.EntryAttemptId,
                        principalTable: "EntryAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AgeCategories",
                columns: new[] { "Id", "DescriptionAr", "IsActive", "NameAr", "SortOrder" },
                values: new object[,]
                {
                    { 1, "13 سنة فأكثر", true, "بالغ", 1 },
                    { 2, "من 3 إلى 12 سنة", true, "طفل", 2 },
                    { 3, "أقل من 3 سنوات", true, "رضيع", 3 }
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Key", "UpdatedAtUtc", "UpdatedByUserId", "Value" },
                values: new object[,]
                {
                    { "Entry.DefaultCooldownMinutes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "720" },
                    { "Entry.DenialDedupMinutes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "5" },
                    { "Entry.MaxGroupSize", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "10" },
                    { "Entry.PlaySoundOnDenial", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "true" },
                    { "Entry.PrefillCompanionsFromLastVisit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "true" },
                    { "Park.Name", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "منتزه المرح العائلي" },
                    { "Park.TimeZoneId", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Libya Standard Time" },
                    { "Security.LockoutMinutes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "15" },
                    { "Security.MaxFailedLogins", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "5" },
                    { "Ui.MaskNationalIdForOperators", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "true" }
                });

            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "Code", "CooldownMinutes", "CreatedAtUtc", "CreatedByUserId", "IsActive", "IsDefault", "NameAr", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[] { 1, "SSF", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "صندوق التضامن الاجتماعي", null, null });

            migrationBuilder.CreateIndex(
                name: "UX_AgeCategories_NameAr",
                table: "AgeCategories",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AtUtc",
                table: "AuditLogs",
                column: "AtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entity",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_CardNumber",
                table: "Beneficiaries",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_FullName",
                table: "Beneficiaries",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_Phone",
                table: "Beneficiaries",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "UX_Beneficiaries_NationalId",
                table: "Beneficiaries",
                column: "NationalId",
                unique: true,
                filter: "[NationalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Beneficiaries_Org_Card",
                table: "Beneficiaries",
                columns: new[] { "OrganizationId", "CardNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttemptCompanions_AgeCategoryId",
                table: "EntryAttemptCompanions",
                column: "AgeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttempts_Beneficiary_Time",
                table: "EntryAttempts",
                columns: new[] { "BeneficiaryId", "AttemptedAtUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttempts_BusinessDate",
                table: "EntryAttempts",
                column: "BusinessDate")
                .Annotation("SqlServer:Include", new[] { "Result", "TotalPersons", "OrganizationId", "IsVoided" });

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttempts_OrganizationId",
                table: "EntryAttempts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttempts_OverrideByUserId",
                table: "EntryAttempts",
                column: "OverrideByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttempts_UserId",
                table: "EntryAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttempts_VoidedByUserId",
                table: "EntryAttempts",
                column: "VoidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttempts_Workstation_Time",
                table: "EntryAttempts",
                columns: new[] { "WorkstationId", "AttemptedAtUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_Organizations_Code",
                table: "Organizations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Organizations_IsDefault",
                table: "Organizations",
                column: "IsDefault",
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Workstations_MachineName",
                table: "Workstations",
                column: "MachineName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EntryAttemptCompanions");

            migrationBuilder.DropTable(
                name: "AgeCategories");

            migrationBuilder.DropTable(
                name: "EntryAttempts");

            migrationBuilder.DropTable(
                name: "Beneficiaries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Workstations");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
