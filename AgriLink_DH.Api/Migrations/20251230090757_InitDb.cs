using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "farms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    area_size = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    address_gps = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_farms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    farm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    default_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    default_price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_task_types_farms_farm_id",
                        column: x => x.farm_id,
                        principalTable: "farms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weather_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    farm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    condition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    rainfall_mm = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_weather_logs_farms_farm_id",
                        column: x => x.farm_id,
                        principalTable: "farms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    worker_type = table.Column<string>(type: "text", nullable: false),
                    default_daily_wage = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    FarmId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workers", x => x.id);
                    table.ForeignKey(
                        name: "FK_workers_farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "farms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "crop_seasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    farm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crop_seasons", x => x.id);
                    table.ForeignKey(
                        name: "FK_crop_seasons_farms_farm_id",
                        column: x => x.farm_id,
                        principalTable: "farms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crop_seasons_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_login_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    login_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    action_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_login_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_login_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_work_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    task_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    total_cost = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_work_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_work_logs_crop_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "crop_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_daily_work_logs_task_types_task_type_id",
                        column: x => x.task_type_id,
                        principalTable: "task_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "farm_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sale_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    buyer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantity_sold = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    price_per_kg = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    total_revenue = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_farm_sales", x => x.id);
                    table.ForeignKey(
                        name: "FK_farm_sales_crop_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "crop_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "harvest_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    harvest_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    total_bags = table.Column<int>(type: "integer", nullable: false),
                    total_weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    storage_location = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_harvest_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_harvest_sessions_crop_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "crop_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usage_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    material_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    unit_price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_usages", x => x.id);
                    table.ForeignKey(
                        name: "FK_material_usages_crop_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "crop_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_advances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    advance_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_deducted = table.Column<bool>(type: "boolean", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_advances", x => x.id);
                    table.ForeignKey(
                        name: "FK_worker_advances_crop_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "crop_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_worker_advances_workers_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_work_assignments_daily_work_logs_log_id",
                        column: x => x.log_id,
                        principalTable: "daily_work_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_work_assignments_workers_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "harvest_bag_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bag_index = table.Column<int>(type: "integer", nullable: false),
                    gross_weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    deduction = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    net_weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_harvest_bag_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_harvest_bag_details_harvest_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "harvest_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "id", "code", "name", "unit" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "CF_ROBUSTA", "Cà phê Robusta", "kg" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "PEPPER", "Hồ Tiêu", "kg" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "DURIAN", "Sầu Riêng", "kg" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_crop_seasons_farm_id_product_id",
                table: "crop_seasons",
                columns: new[] { "farm_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "IX_crop_seasons_product_id",
                table: "crop_seasons",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_work_logs_season_id_work_date",
                table: "daily_work_logs",
                columns: new[] { "season_id", "work_date" });

            migrationBuilder.CreateIndex(
                name: "IX_daily_work_logs_task_type_id",
                table: "daily_work_logs",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_farm_sales_season_id_sale_date",
                table: "farm_sales",
                columns: new[] { "season_id", "sale_date" });

            migrationBuilder.CreateIndex(
                name: "IX_farms_owner_user_id",
                table: "farms",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_harvest_bag_details_session_id",
                table: "harvest_bag_details",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_harvest_sessions_season_id_harvest_date",
                table: "harvest_sessions",
                columns: new[] { "season_id", "harvest_date" });

            migrationBuilder.CreateIndex(
                name: "IX_material_usages_season_id",
                table: "material_usages",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_code",
                table: "products",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_types_farm_id",
                table: "task_types",
                column: "farm_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_login_logs_user_id_login_time",
                table: "user_login_logs",
                columns: new[] { "user_id", "login_time" });

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weather_logs_farm_id",
                table: "weather_logs",
                column: "farm_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_assignments_log_id",
                table: "work_assignments",
                column: "log_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_assignments_worker_id",
                table: "work_assignments",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "IX_worker_advances_season_id",
                table: "worker_advances",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "IX_worker_advances_worker_id",
                table: "worker_advances",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "IX_workers_FarmId",
                table: "workers",
                column: "FarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "farm_sales");

            migrationBuilder.DropTable(
                name: "harvest_bag_details");

            migrationBuilder.DropTable(
                name: "material_usages");

            migrationBuilder.DropTable(
                name: "user_login_logs");

            migrationBuilder.DropTable(
                name: "weather_logs");

            migrationBuilder.DropTable(
                name: "work_assignments");

            migrationBuilder.DropTable(
                name: "worker_advances");

            migrationBuilder.DropTable(
                name: "harvest_sessions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "daily_work_logs");

            migrationBuilder.DropTable(
                name: "workers");

            migrationBuilder.DropTable(
                name: "crop_seasons");

            migrationBuilder.DropTable(
                name: "task_types");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "farms");
        }
    }
}
