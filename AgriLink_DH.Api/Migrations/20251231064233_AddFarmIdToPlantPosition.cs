using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmIdToPlantPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_plant_positions_crop_seasons_season_id",
                table: "plant_positions");

            migrationBuilder.AlterColumn<Guid>(
                name: "season_id",
                table: "plant_positions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "farm_id",
                table: "plant_positions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_plant_positions_farm_id",
                table: "plant_positions",
                column: "farm_id");

            migrationBuilder.AddForeignKey(
                name: "FK_plant_positions_crop_seasons_season_id",
                table: "plant_positions",
                column: "season_id",
                principalTable: "crop_seasons",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_plant_positions_farms_farm_id",
                table: "plant_positions",
                column: "farm_id",
                principalTable: "farms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_plant_positions_crop_seasons_season_id",
                table: "plant_positions");

            migrationBuilder.DropForeignKey(
                name: "FK_plant_positions_farms_farm_id",
                table: "plant_positions");

            migrationBuilder.DropIndex(
                name: "IX_plant_positions_farm_id",
                table: "plant_positions");

            migrationBuilder.DropColumn(
                name: "farm_id",
                table: "plant_positions");

            migrationBuilder.AlterColumn<Guid>(
                name: "season_id",
                table: "plant_positions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_plant_positions_crop_seasons_season_id",
                table: "plant_positions",
                column: "season_id",
                principalTable: "crop_seasons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
