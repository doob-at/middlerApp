using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace middlerApp.DataAccess.Sqlite.Migrations
{
    public partial class _20210927T202732 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EndpointRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<decimal>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Scheme = table.Column<string>(type: "TEXT", nullable: true),
                    Hostname = table.Column<string>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    HttpMethods = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Module = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    IsFolder = table.Column<bool>(type: "INTEGER", nullable: false),
                    Extension = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Bytes = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EndpointActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<decimal>(type: "TEXT", nullable: false),
                    EndpointRuleEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Terminating = table.Column<bool>(type: "INTEGER", nullable: false),
                    WriteStreamDirect = table.Column<bool>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", nullable: true),
                    Parameters = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndpointActions_EndpointRules_EndpointRuleEntityId",
                        column: x => x.EndpointRuleEntityId,
                        principalTable: "EndpointRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EndpointRulePermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrincipalName = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    AccessMode = table.Column<string>(type: "TEXT", nullable: true),
                    Client = table.Column<string>(type: "TEXT", nullable: true),
                    SourceAddress = table.Column<string>(type: "TEXT", nullable: true),
                    EndpointRuleEntityId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointRulePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndpointRulePermission_EndpointRules_EndpointRuleEntityId",
                        column: x => x.EndpointRuleEntityId,
                        principalTable: "EndpointRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EndpointActions_EndpointRuleEntityId",
                table: "EndpointActions",
                column: "EndpointRuleEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EndpointRulePermission_EndpointRuleEntityId",
                table: "EndpointRulePermission",
                column: "EndpointRuleEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EndpointActions");

            migrationBuilder.DropTable(
                name: "EndpointRulePermission");

            migrationBuilder.DropTable(
                name: "TypeDefinitions");

            migrationBuilder.DropTable(
                name: "Variables");

            migrationBuilder.DropTable(
                name: "EndpointRules");
        }
    }
}
