using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnreadSoundSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Key",
                keyValue: "Entry.PlaySoundOnDenial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Key", "UpdatedAtUtc", "UpdatedByUserId", "Value" },
                values: new object[] { "Entry.PlaySoundOnDenial", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "true" });
        }
    }
}
