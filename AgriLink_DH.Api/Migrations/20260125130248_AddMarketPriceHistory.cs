using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "market_price_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    product_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    region_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    change = table.Column<decimal>(type: "numeric", nullable: false),
                    change_percent = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    recorded_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_price_history", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "market_price_history",
                columns: new[] { "id", "change", "change_percent", "created_at", "notes", "price", "product_name", "product_type", "recorded_date", "region", "region_code", "source", "unit", "updated_by" },
                values: new object[,]
                {
                    { 1, 1700m, 1.71m, new DateTime(2026, 1, 25, 13, 2, 48, 51, DateTimeKind.Utc).AddTicks(8602), "Seed data", 100900m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đắk Lắk", "DAK_LAK", "Admin", "VND/kg", "System" },
                    { 2, 1700m, 1.72m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(583), "Seed data", 100500m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lâm Đồng", "LAM_DONG", "Admin", "VND/kg", "System" },
                    { 3, 1600m, 1.61m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(587), "Seed data", 100800m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gia Lai", "GIA_LAI", "Admin", "VND/kg", "System" },
                    { 4, 1700m, 1.71m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(590), "Seed data", 101000m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đắk Nông", "DAK_NONG", "Admin", "VND/kg", "System" },
                    { 5, -800m, -0.80m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(591), "Seed data", 99200m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đắk Lắk", "DAK_LAK", "Admin", "VND/kg", "System" },
                    { 6, -700m, -0.70m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(593), "Seed data", 98800m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lâm Đồng", "LAM_DONG", "Admin", "VND/kg", "System" },
                    { 7, -800m, -0.80m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(596), "Seed data", 99200m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gia Lai", "GIA_LAI", "Admin", "VND/kg", "System" },
                    { 8, -900m, -0.90m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(598), "Seed data", 99300m, "Cà phê Robusta", "COFFEE", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đắk Nông", "DAK_NONG", "Admin", "VND/kg", "System" },
                    { 9, 0m, 0m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(600), "Seed data", 149000m, "Hồ tiêu", "PEPPER", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đắk Lắk", "DAK_LAK", "Admin", "VND/kg", "System" },
                    { 10, 0m, 0m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(602), "Seed data", 149000m, "Hồ tiêu", "PEPPER", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lâm Đồng", "LAM_DONG", "Admin", "VND/kg", "System" },
                    { 11, 0m, 0m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(603), "Seed data", 149000m, "Hồ tiêu", "PEPPER", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gia Lai", "GIA_LAI", "Admin", "VND/kg", "System" },
                    { 12, 0m, 0m, new DateTime(2026, 1, 25, 13, 2, 48, 52, DateTimeKind.Utc).AddTicks(605), "Seed data", 149000m, "Hồ tiêu", "PEPPER", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đắk Nông", "DAK_NONG", "Admin", "VND/kg", "System" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_market_price_history_product_type_region_code_recorded_date",
                table: "market_price_history",
                columns: new[] { "product_type", "region_code", "recorded_date" });

            migrationBuilder.CreateIndex(
                name: "IX_market_price_history_recorded_date",
                table: "market_price_history",
                column: "recorded_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "market_price_history");
        }
    }
}
