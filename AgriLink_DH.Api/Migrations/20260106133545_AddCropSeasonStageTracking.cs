using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCropSeasonStageTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "current_stage",
                table: "crop_seasons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "stage_changed_at",
                table: "crop_seasons",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stage_notes",
                table: "crop_seasons",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_stage",
                table: "crop_seasons");

            migrationBuilder.DropColumn(
                name: "stage_changed_at",
                table: "crop_seasons");

            migrationBuilder.DropColumn(
                name: "stage_notes",
                table: "crop_seasons");
        }
    }
}
