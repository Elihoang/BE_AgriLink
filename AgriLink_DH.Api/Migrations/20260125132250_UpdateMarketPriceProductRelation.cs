using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarketPriceProductRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_market_price_history_product_type_region_code_recorded_date",
                table: "market_price_history");

            migrationBuilder.DropColumn(
                name: "product_name",
                table: "market_price_history");

            migrationBuilder.DropColumn(
                name: "product_type",
                table: "market_price_history");

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                table: "market_price_history",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_market_price_history_product_id_region_code_recorded_date",
                table: "market_price_history",
                columns: new[] { "product_id", "region_code", "recorded_date" });

            migrationBuilder.AddForeignKey(
                name: "FK_market_price_history_products_product_id",
                table: "market_price_history",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_market_price_history_products_product_id",
                table: "market_price_history");

            migrationBuilder.DropIndex(
                name: "IX_market_price_history_product_id_region_code_recorded_date",
                table: "market_price_history");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "market_price_history");

            migrationBuilder.AddColumn<string>(
                name: "product_name",
                table: "market_price_history",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "product_type",
                table: "market_price_history",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_market_price_history_product_type_region_code_recorded_date",
                table: "market_price_history",
                columns: new[] { "product_type", "region_code", "recorded_date" });
        }
    }
}
