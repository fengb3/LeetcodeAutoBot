using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetcodeAutoBot.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceToCookie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "Cookies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Device",
                table: "Cookies");
        }
    }
}
