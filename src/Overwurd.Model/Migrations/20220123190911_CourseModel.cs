using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Overwurd.Model.Migrations
{
    public partial class CourseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vocabularies_Name",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                schema: "overwurd",
                table: "Vocabularies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "overwurd",
                table: "Vocabularies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Courses",
                schema: "overwurd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "overwurd",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_CourseId",
                schema: "overwurd",
                table: "Vocabularies",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_Name_Id",
                schema: "overwurd",
                table: "Vocabularies",
                columns: new[] { "Name", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Name_Id",
                schema: "overwurd",
                table: "Courses",
                columns: new[] { "Name", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserId",
                schema: "overwurd",
                table: "Courses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vocabularies_Courses_CourseId",
                schema: "overwurd",
                table: "Vocabularies",
                column: "CourseId",
                principalSchema: "overwurd",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vocabularies_Courses_CourseId",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.DropTable(
                name: "Courses",
                schema: "overwurd");

            migrationBuilder.DropIndex(
                name: "IX_Vocabularies_CourseId",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.DropIndex(
                name: "IX_Vocabularies_Name_Id",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.DropColumn(
                name: "CourseId",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_Name",
                schema: "overwurd",
                table: "Vocabularies",
                column: "Name",
                unique: true);
        }
    }
}
