using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateMarketPriceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "market_price_history",
                keyColumn: "id",
                keyValue: 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
