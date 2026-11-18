using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdoptEntryCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardScanLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScannedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    StationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkstationId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RawPrefix = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardScanLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardScanLog_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QrSigningKeys",
                columns: table => new
                {
                    KeyId = table.Column<byte>(type: "tinyint", nullable: false),
                    PublicKeySpki = table.Column<byte[]>(type: "varbinary(200)", maxLength: 200, nullable: false),
                    StationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrSigningKeys", x => x.KeyId);
                });

            migrationBuilder.CreateTable(
                name: "EntryCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BeneficiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyId = table.Column<byte>(type: "tinyint", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    RevokeReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryCards_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryCards_QrSigningKeys_KeyId",
                        column: x => x.KeyId,
                        principalTable: "QrSigningKeys",
                        principalColumn: "KeyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardScanLog_CardId",
                table: "CardScanLog",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardScanLog_ScannedAtUtc",
                table: "CardScanLog",
                column: "ScannedAtUtc")
                .Annotation("SqlServer:Include", new[] { "Status", "StationName" });

            migrationBuilder.CreateIndex(
                name: "IX_CardScanLog_WorkstationId",
                table: "CardScanLog",
                column: "WorkstationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryCards_KeyId",
                table: "EntryCards",
                column: "KeyId");

            migrationBuilder.CreateIndex(
                name: "UX_EntryCards_CardId",
                table: "EntryCards",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_EntryCards_OneActivePerBeneficiary",
                table: "EntryCards",
                column: "BeneficiaryId",
                unique: true,
                filter: "[RevokedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_QrSigningKeys_PublicKeySpki",
                table: "QrSigningKeys",
                column: "PublicKeySpki",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardScanLog");

            migrationBuilder.DropTable(
                name: "EntryCards");

            migrationBuilder.DropTable(
                name: "QrSigningKeys");
        }
    }
}
