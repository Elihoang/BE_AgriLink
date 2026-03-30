using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriLink_DH.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBluetoothScaleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_auto_weighed",
                table: "harvest_bag_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "scale_device_id",
                table: "harvest_bag_details",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_auto_weighed",
                table: "harvest_bag_details");

            migrationBuilder.DropColumn(
                name: "scale_device_id",
                table: "harvest_bag_details");
        }
    }
}
