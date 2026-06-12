using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemTaskTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "task_types",
                columns: new[] { "id", "default_price", "default_unit", "farm_id", "is_system", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-000000000001"), 250000m, "Ngày công", null, true, "Làm đất" },
                    { new Guid("11111111-1111-1111-1111-000000000002"), 100000m, "Lần", null, true, "Xuống giống / Gieo hạt" },
                    { new Guid("11111111-1111-1111-1111-000000000003"), 50000m, "Giờ", null, true, "Tưới nước" },
                    { new Guid("11111111-1111-1111-1111-000000000004"), 150000m, "Lần", null, true, "Bón phân" },
                    { new Guid("11111111-1111-1111-1111-000000000005"), 200000m, "Ngày công", null, true, "Làm cỏ" },
                    { new Guid("11111111-1111-1111-1111-000000000006"), 80000m, "Bình", null, true, "Phun thuốc (BVTV)" },
                    { new Guid("11111111-1111-1111-1111-000000000007"), 300000m, "Ngày công", null, true, "Cắt tỉa cành / Tạo tán" },
                    { new Guid("11111111-1111-1111-1111-000000000008"), 300000m, "Ngày công", null, true, "Thu hoạch" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000001"));

            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000002"));

            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000003"));

            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000004"));

            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000005"));

            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000006"));

            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000007"));

            migrationBuilder.DeleteData(
                table: "task_types",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000008"));
        }
    }
}
