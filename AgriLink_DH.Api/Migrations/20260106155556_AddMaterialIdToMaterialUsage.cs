using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialIdToMaterialUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "material_id",
                table: "material_usages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_material_usages_material_id",
                table: "material_usages",
                column: "material_id");

            migrationBuilder.AddForeignKey(
                name: "FK_material_usages_Material_material_id",
                table: "material_usages",
                column: "material_id",
                principalTable: "Material",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_material_usages_Material_material_id",
                table: "material_usages");

            migrationBuilder.DropIndex(
                name: "IX_material_usages_material_id",
                table: "material_usages");

            migrationBuilder.DropColumn(
                name: "material_id",
                table: "material_usages");
        }
    }
}
