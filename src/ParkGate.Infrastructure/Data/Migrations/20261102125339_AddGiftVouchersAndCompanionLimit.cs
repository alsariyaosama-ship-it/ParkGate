using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftVouchersAndCompanionLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GiftEnabled",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GiftIntervalDays",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<decimal>(
                name: "GiftValue",
                table: "Organizations",
                type: "decimal(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxFreeCompanions",
                table: "Organizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompanionLimitExempt",
                table: "AgeCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GiftVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyId = table.Column<byte>(type: "tinyint", nullable: false),
                    BeneficiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IssuedByUserId = table.Column<int>(type: "int", nullable: false),
                    IssuedWorkstationId = table.Column<int>(type: "int", nullable: false),
                    IsOverride = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OverrideByUserId = table.Column<int>(type: "int", nullable: true),
                    OverrideReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RedeemedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    RedeemedBusinessDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RedeemedByUserId = table.Column<int>(type: "int", nullable: true),
                    RedeemedWorkstationId = table.Column<int>(type: "int", nullable: true),
                    VoidedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    VoidedByUserId = table.Column<int>(type: "int", nullable: true),
                    VoidReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftVouchers", x => x.Id);
                    table.CheckConstraint("CK_GiftVouchers_Override", "([IsOverride] = 0 AND [OverrideByUserId] IS NULL AND [OverrideReason] IS NULL) OR ([IsOverride] = 1 AND [OverrideByUserId] IS NOT NULL AND [OverrideReason] IS NOT NULL)");
                    table.CheckConstraint("CK_GiftVouchers_Status", "([Status] = 1 AND [RedeemedAtUtc] IS NULL AND [VoidedAtUtc] IS NULL) OR ([Status] = 2 AND [RedeemedAtUtc] IS NOT NULL AND [RedeemedBusinessDate] IS NOT NULL AND [RedeemedByUserId] IS NOT NULL AND [RedeemedWorkstationId] IS NOT NULL AND [VoidedAtUtc] IS NULL) OR ([Status] = 3 AND [VoidedAtUtc] IS NOT NULL AND [VoidedByUserId] IS NOT NULL AND [VoidReason] IS NOT NULL AND [RedeemedAtUtc] IS NULL)");
                    table.CheckConstraint("CK_GiftVouchers_Value", "[Value] > 0");
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Users_OverrideByUserId",
                        column: x => x.OverrideByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Users_RedeemedByUserId",
                        column: x => x.RedeemedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Users_VoidedByUserId",
                        column: x => x.VoidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Workstations_IssuedWorkstationId",
                        column: x => x.IssuedWorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftVouchers_Workstations_RedeemedWorkstationId",
                        column: x => x.RedeemedWorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AgeCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsCompanionLimitExempt",
                value: true);

            migrationBuilder.UpdateData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GiftIntervalDays", "MaxFreeCompanions" },
                values: new object[] { 30, null });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organizations_GiftIntervalDays",
                table: "Organizations",
                sql: "[GiftIntervalDays] >= 1 AND [GiftIntervalDays] <= 365");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organizations_GiftValue",
                table: "Organizations",
                sql: "[GiftEnabled] = 0 OR [GiftValue] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organizations_MaxFreeCompanions",
                table: "Organizations",
                sql: "[MaxFreeCompanions] IS NULL OR ([MaxFreeCompanions] >= 0 AND [MaxFreeCompanions] <= 50)");

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_Beneficiary_Time",
                table: "GiftVouchers",
                columns: new[] { "BeneficiaryId", "IssuedAtUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_BusinessDate",
                table: "GiftVouchers",
                column: "BusinessDate")
                .Annotation("SqlServer:Include", new[] { "Status", "Value", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_IssuedByUserId",
                table: "GiftVouchers",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_IssuedWorkstationId",
                table: "GiftVouchers",
                column: "IssuedWorkstationId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_OrganizationId",
                table: "GiftVouchers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_OverrideByUserId",
                table: "GiftVouchers",
                column: "OverrideByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_RedeemedBusinessDate",
                table: "GiftVouchers",
                column: "RedeemedBusinessDate",
                filter: "[RedeemedBusinessDate] IS NOT NULL")
                .Annotation("SqlServer:Include", new[] { "Value", "OrganizationId", "RedeemedWorkstationId" });

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_RedeemedByUserId",
                table: "GiftVouchers",
                column: "RedeemedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_RedeemedWorkstationId",
                table: "GiftVouchers",
                column: "RedeemedWorkstationId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftVouchers_VoidedByUserId",
                table: "GiftVouchers",
                column: "VoidedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_GiftVouchers_Token",
                table: "GiftVouchers",
                column: "Token",
                unique: true);

            // BR-26: the existing «رضيع» category must not count toward the free-companion limit.
            // Seeded data only covers new databases; this fixes databases that already exist.
            migrationBuilder.Sql("UPDATE dbo.AgeCategories SET [IsCompanionLimitExempt] = 1 WHERE [NameAr] = N'رضيع';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftVouchers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Organizations_GiftIntervalDays",
                table: "Organizations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Organizations_GiftValue",
                table: "Organizations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Organizations_MaxFreeCompanions",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GiftEnabled",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GiftIntervalDays",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GiftValue",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "MaxFreeCompanions",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IsCompanionLimitExempt",
                table: "AgeCategories");
        }
    }
}
