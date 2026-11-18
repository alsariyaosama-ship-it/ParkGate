using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedMaskSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Key",
                keyValue: "Ui.MaskNationalIdForOperators");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Key", "UpdatedAtUtc", "UpdatedByUserId", "Value" },
                values: new object[] { "Ui.MaskNationalIdForOperators", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "true" });
        }
    }
}
