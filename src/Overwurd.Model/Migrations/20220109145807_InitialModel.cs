using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Overwurd.Model.Migrations
{
    public partial class InitialModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "overwurd");

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "overwurd",
                columns: table => new
                {
                    RoleType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleType);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "overwurd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    SecondName = table.Column<string>(type: "text", nullable: true),
                    Login = table.Column<string>(type: "text", nullable: true),
                    NormalizedLogin = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vocabularies",
                schema: "overwurd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vocabularies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JwtRefreshTokens",
                schema: "overwurd",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccessTokenId = table.Column<string>(type: "text", nullable: false),
                    TokenString = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JwtRefreshTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_JwtRefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "overwurd",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                schema: "overwurd",
                columns: table => new
                {
                    RolesRoleType = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesRoleType, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Role_RolesRoleType",
                        column: x => x.RolesRoleType,
                        principalSchema: "overwurd",
                        principalTable: "Role",
                        principalColumn: "RoleType",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalSchema: "overwurd",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "overwurd",
                table: "Role",
                columns: new[] { "RoleType", "Name" },
                values: new object[] { 0, "Administrator" });

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                schema: "overwurd",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                schema: "overwurd",
                table: "RoleUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                schema: "overwurd",
                table: "Users",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedLogin",
                schema: "overwurd",
                table: "Users",
                column: "NormalizedLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_Name",
                schema: "overwurd",
                table: "Vocabularies",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JwtRefreshTokens",
                schema: "overwurd");

            migrationBuilder.DropTable(
                name: "RoleUser",
                schema: "overwurd");

            migrationBuilder.DropTable(
                name: "Vocabularies",
                schema: "overwurd");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "overwurd");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "overwurd");
        }
    }
}
