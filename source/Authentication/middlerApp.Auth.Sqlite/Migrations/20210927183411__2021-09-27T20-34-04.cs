using Microsoft.EntityFrameworkCore.Migrations;

namespace middlerApp.Auth.Sqlite.Migrations
{
    public partial class _20210927T203404 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Roles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Roles",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Roles");
        }
    }
}
