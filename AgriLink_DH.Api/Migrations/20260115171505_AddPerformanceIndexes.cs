using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_workers_is_active",
                table: "workers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_worker_advances_is_deducted",
                table: "worker_advances",
                column: "is_deducted");

            migrationBuilder.CreateIndex(
                name: "IX_worker_advances_worker_id_season_id",
                table: "worker_advances",
                columns: new[] { "worker_id", "season_id" });

            migrationBuilder.CreateIndex(
                name: "IX_weather_logs_farm_id_log_date",
                table: "weather_logs",
                columns: new[] { "farm_id", "log_date" });

            migrationBuilder.CreateIndex(
                name: "IX_material_usages_is_deleted",
                table: "material_usages",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_material_usages_season_id_usage_date",
                table: "material_usages",
                columns: new[] { "season_id", "usage_date" });

            migrationBuilder.CreateIndex(
                name: "IX_farms_is_deleted",
                table: "farms",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_daily_work_logs_is_deleted",
                table: "daily_work_logs",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_crop_seasons_is_deleted",
                table: "crop_seasons",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_crop_seasons_status",
                table: "crop_seasons",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workers_is_active",
                table: "workers");

            migrationBuilder.DropIndex(
                name: "IX_worker_advances_is_deducted",
                table: "worker_advances");

            migrationBuilder.DropIndex(
                name: "IX_worker_advances_worker_id_season_id",
                table: "worker_advances");

            migrationBuilder.DropIndex(
                name: "IX_weather_logs_farm_id_log_date",
                table: "weather_logs");

            migrationBuilder.DropIndex(
                name: "IX_material_usages_is_deleted",
                table: "material_usages");

            migrationBuilder.DropIndex(
                name: "IX_material_usages_season_id_usage_date",
                table: "material_usages");

            migrationBuilder.DropIndex(
                name: "IX_farms_is_deleted",
                table: "farms");

            migrationBuilder.DropIndex(
                name: "IX_daily_work_logs_is_deleted",
                table: "daily_work_logs");

            migrationBuilder.DropIndex(
                name: "IX_crop_seasons_is_deleted",
                table: "crop_seasons");

            migrationBuilder.DropIndex(
                name: "IX_crop_seasons_status",
                table: "crop_seasons");
        }
    }
}
