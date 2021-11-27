using Microsoft.EntityFrameworkCore.Migrations;

namespace middlerApp.Auth.Postgres.Migrations
{
    public partial class _20210927T203404 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Roles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Roles",
                type: "text",
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
