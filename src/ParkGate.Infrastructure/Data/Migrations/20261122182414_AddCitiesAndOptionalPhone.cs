using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCitiesAndOptionalPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Beneficiaries",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Beneficiaries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "IsActive", "NameAr", "SortOrder" },
                values: new object[,]
                {
                    { 1, true, "الخمس", 1 },
                    { 2, true, "زليتن", 2 },
                    { 3, true, "مصراتة", 3 },
                    { 4, true, "طرابلس", 4 },
                    { 5, true, "مسلاتة", 5 },
                    { 6, true, "ترهونة", 6 },
                    { 7, true, "القره بوللي", 7 },
                    { 8, true, "قصر الأخيار", 8 },
                    { 9, true, "العلوص", 9 },
                    { 10, true, "الثمانين", 10 },
                    { 11, true, "قماطة", 11 },
                    { 12, true, "بني وليد", 12 },
                    { 13, true, "سرت", 13 },
                    { 14, true, "العجيلات", 14 },
                    { 15, true, "صبراتة", 15 },
                    { 16, true, "جنزور", 16 },
                    { 17, true, "أبو سليم", 17 },
                    { 18, true, "أبو كماش", 18 },
                    { 19, true, "الأصابعة", 19 },
                    { 20, true, "الجميل", 20 },
                    { 21, true, "الرجبان", 21 },
                    { 22, true, "الرحيبات", 22 },
                    { 23, true, "الزاوية", 23 },
                    { 24, true, "الزنتان", 24 },
                    { 25, true, "الزهراء", 25 },
                    { 26, true, "السبيعة", 26 },
                    { 27, true, "السواني", 27 },
                    { 28, true, "العزيزية", 28 },
                    { 29, true, "القلعة", 29 },
                    { 30, true, "الماية", 30 },
                    { 31, true, "بئر الغنم", 31 },
                    { 32, true, "تاجوراء", 32 },
                    { 33, true, "تاورغاء", 33 },
                    { 34, true, "جادو", 34 },
                    { 35, true, "حي الأندلس", 35 },
                    { 36, true, "درج", 36 },
                    { 37, true, "رقدالين", 37 },
                    { 38, true, "زلطن", 38 },
                    { 39, true, "زوارة", 39 },
                    { 40, true, "سبها", 40 },
                    { 41, true, "سوق الجمعة", 41 },
                    { 42, true, "سوق الخميس امسيحل", 42 },
                    { 43, true, "سوكنة", 43 },
                    { 44, true, "صرمان", 44 },
                    { 45, true, "عين زارة", 45 },
                    { 46, true, "غدامس", 46 },
                    { 47, true, "غريان", 47 },
                    { 48, true, "قصر بن غشير", 48 },
                    { 49, true, "كاباو", 49 },
                    { 50, true, "كعام", 50 },
                    { 51, true, "ككلة", 51 },
                    { 52, true, "ماجر", 52 },
                    { 53, true, "مزدة", 53 },
                    { 54, true, "نالوت", 54 },
                    { 55, true, "هون", 55 },
                    { 56, true, "ودان", 56 },
                    { 57, true, "يفرن", 57 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_CityId",
                table: "Beneficiaries",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "UX_Cities_NameAr",
                table: "Cities",
                column: "NameAr",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_Cities_CityId",
                table: "Beneficiaries",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_Cities_CityId",
                table: "Beneficiaries");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_CityId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Beneficiaries");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Beneficiaries",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);
        }
    }
}
