using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetcodeAutoBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cookies",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    Expires = table.Column<float>(type: "REAL", nullable: true),
                    HttpOnly = table.Column<bool>(type: "INTEGER", nullable: true),
                    Secure = table.Column<bool>(type: "INTEGER", nullable: true),
                    SameSite = table.Column<int>(type: "INTEGER", nullable: true),
                    PartitionKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cookies", x => new { x.Name, x.Domain, x.AccountId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cookies");
        }
    }
}
