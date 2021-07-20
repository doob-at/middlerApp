using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace middlerApp.DataAccess.SqlServer.Migrations
{
    public partial class _20210708T105326 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EndpointRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Scheme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hostname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HttpMethods = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Parent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFolder = table.Column<bool>(type: "bit", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EndpointActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndpointRuleEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Terminating = table.Column<bool>(type: "bit", nullable: false),
                    WriteStreamDirect = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrincipalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Client = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndpointRuleEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
