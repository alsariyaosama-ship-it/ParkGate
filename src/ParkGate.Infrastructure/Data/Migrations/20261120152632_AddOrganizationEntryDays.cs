using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationEntryDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "AllowedEntryDays",
                table: "Organizations",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)127);

            migrationBuilder.AddColumn<bool>(
                name: "GiftAnyDay",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: 1,
                column: "AllowedEntryDays",
                value: (byte)127);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organizations_AllowedEntryDays",
                table: "Organizations",
                sql: "[AllowedEntryDays] >= 1 AND [AllowedEntryDays] <= 127");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Organizations_AllowedEntryDays",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "AllowedEntryDays",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GiftAnyDay",
                table: "Organizations");
        }
    }
}
