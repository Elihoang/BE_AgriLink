using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeTaskTypeHybrid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_types_farms_farm_id",
                table: "task_types");

            migrationBuilder.AlterColumn<Guid>(
                name: "farm_id",
                table: "task_types",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "is_system",
                table: "task_types",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_task_types_farms_farm_id",
                table: "task_types",
                column: "farm_id",
                principalTable: "farms",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_types_farms_farm_id",
                table: "task_types");

            migrationBuilder.DropColumn(
                name: "is_system",
                table: "task_types");

            migrationBuilder.AlterColumn<Guid>(
                name: "farm_id",
                table: "task_types",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_task_types_farms_farm_id",
                table: "task_types",
                column: "farm_id",
                principalTable: "farms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
