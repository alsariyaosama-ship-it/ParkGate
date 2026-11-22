using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryReceiptNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiptNo",
                table: "EntryAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "UX_EntryAttempts_ReceiptNo",
                table: "EntryAttempts",
                column: "ReceiptNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_EntryAttempts_ReceiptNo",
                table: "EntryAttempts");

            migrationBuilder.DropColumn(
                name: "ReceiptNo",
                table: "EntryAttempts");
        }
    }
}
