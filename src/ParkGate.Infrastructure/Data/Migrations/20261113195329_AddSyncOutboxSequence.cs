using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncOutboxSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncOutboxes_Pending",
                table: "SyncOutboxes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "SyncOutboxes",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3);

            migrationBuilder.AddColumn<long>(
                name: "Sequence",
                table: "SyncOutboxes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_SyncOutboxes_Pending",
                table: "SyncOutboxes",
                columns: new[] { "IsProcessed", "RetryCount", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncOutboxes_Pending",
                table: "SyncOutboxes");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "SyncOutboxes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "SyncOutboxes",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3,
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.CreateIndex(
                name: "IX_SyncOutboxes_Pending",
                table: "SyncOutboxes",
                columns: new[] { "IsProcessed", "RetryCount", "CreatedAtUtc" });
        }
    }
}
