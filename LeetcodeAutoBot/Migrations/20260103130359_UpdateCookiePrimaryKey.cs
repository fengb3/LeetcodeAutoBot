using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetcodeAutoBot.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCookiePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies",
                columns: new[] { "Name", "Domain", "AccountId", "Device" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cookies",
                table: "Cookies",
                columns: new[] { "Name", "Domain", "AccountId" });
        }
    }
}
