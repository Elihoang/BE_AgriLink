using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialTypeAndExpiryDateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Material_users_OwnerUserId",
                table: "Material");

            migrationBuilder.DropForeignKey(
                name: "FK_material_usages_Material_material_id",
                table: "material_usages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Material",
                table: "Material");

            migrationBuilder.RenameTable(
                name: "Material",
                newName: "materials");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "materials",
                newName: "unit");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "materials",
                newName: "note");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "materials",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "materials",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "materials",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "QuantityInStock",
                table: "materials",
                newName: "quantity_in_stock");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "materials",
                newName: "owner_user_id");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "materials",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "materials",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CostPerUnit",
                table: "materials",
                newName: "cost_per_unit");

            migrationBuilder.RenameIndex(
                name: "IX_Material_OwnerUserId_Name",
                table: "materials",
                newName: "IX_materials_owner_user_id_name");

            migrationBuilder.AddColumn<DateTime>(
                name: "expiry_date",
                table: "materials",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "material_type",
                table: "materials",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_materials",
                table: "materials",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_material_usages_materials_material_id",
                table: "material_usages",
                column: "material_id",
                principalTable: "materials",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_materials_users_owner_user_id",
                table: "materials",
                column: "owner_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_material_usages_materials_material_id",
                table: "material_usages");

            migrationBuilder.DropForeignKey(
                name: "FK_materials_users_owner_user_id",
                table: "materials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_materials",
                table: "materials");

            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "materials");

            migrationBuilder.DropColumn(
                name: "material_type",
                table: "materials");

            migrationBuilder.RenameTable(
                name: "materials",
                newName: "Material");

            migrationBuilder.RenameColumn(
                name: "unit",
                table: "Material",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "Material",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Material",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Material",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Material",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "quantity_in_stock",
                table: "Material",
                newName: "QuantityInStock");

            migrationBuilder.RenameColumn(
                name: "owner_user_id",
                table: "Material",
                newName: "OwnerUserId");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "Material",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Material",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "cost_per_unit",
                table: "Material",
                newName: "CostPerUnit");

            migrationBuilder.RenameIndex(
                name: "IX_materials_owner_user_id_name",
                table: "Material",
                newName: "IX_Material_OwnerUserId_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Material",
                table: "Material",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Material_users_OwnerUserId",
                table: "Material",
                column: "OwnerUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_material_usages_Material_material_id",
                table: "material_usages",
                column: "material_id",
                principalTable: "Material",
                principalColumn: "Id");
        }
    }
}
