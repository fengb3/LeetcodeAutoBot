using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetcodeAutoBot.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalStorageEntries",
                table: "LocalStorageEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies");

            migrationBuilder.DropColumn(
                name: "Device",
                table: "LocalStorageEntries");

            migrationBuilder.DropColumn(
                name: "Device",
                table: "Cookies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalStorageEntries",
                table: "LocalStorageEntries",
                columns: new[] { "Key", "AccountId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies",
                columns: new[] { "Name", "Domain", "AccountId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalStorageEntries",
                table: "LocalStorageEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies");

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "LocalStorageEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "Cookies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalStorageEntries",
                table: "LocalStorageEntries",
                columns: new[] { "Key", "AccountId", "Device" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies",
                columns: new[] { "Name", "Domain", "AccountId", "Device" });
        }
    }
}
