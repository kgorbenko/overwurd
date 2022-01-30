using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Overwurd.Model.Migrations
{
    public partial class FixUniqueConstraintForCourseAndVocabulary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vocabularies_Name_Id",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Name_Id",
                schema: "overwurd",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_Name_CourseId",
                schema: "overwurd",
                table: "Vocabularies",
                columns: new[] { "Name", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Name_UserId",
                schema: "overwurd",
                table: "Courses",
                columns: new[] { "Name", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vocabularies_Name_CourseId",
                schema: "overwurd",
                table: "Vocabularies");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Name_UserId",
                schema: "overwurd",
                table: "Courses");

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
        }
    }
}
