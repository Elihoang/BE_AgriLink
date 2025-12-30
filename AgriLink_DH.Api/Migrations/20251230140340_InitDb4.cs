using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitDb4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plant_positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_number = table.Column<int>(type: "integer", nullable: false),
                    column_number = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plant_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    health_status = table.Column<int>(type: "integer", nullable: false),
                    estimated_yield = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant_positions", x => x.id);
                    table.ForeignKey(
                        name: "FK_plant_positions_crop_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "crop_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plant_positions_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_plant_positions_product_id",
                table: "plant_positions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_plant_positions_season_id_row_number_column_number",
                table: "plant_positions",
                columns: new[] { "season_id", "row_number", "column_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plant_positions");
        }
    }
}
