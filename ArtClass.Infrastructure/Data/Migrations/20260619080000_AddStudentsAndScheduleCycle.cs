using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtClass.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentsAndScheduleCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRepeating",
                table: "StudyGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "CycleWeek",
                table: "Lessons",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SpecificDate",
                table: "Lessons",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduleSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CycleStartDate = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Age = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentStudyGroups",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudyGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentStudyGroups", x => new { x.StudentId, x.StudyGroupId });
                    table.ForeignKey(
                        name: "FK_StudentStudyGroups_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentStudyGroups_StudyGroups_StudyGroupId",
                        column: x => x.StudyGroupId,
                        principalTable: "StudyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_SpecificDate",
                table: "Lessons",
                column: "SpecificDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudentStudyGroups_StudyGroupId",
                table: "StudentStudyGroups",
                column: "StudyGroupId");

            migrationBuilder.Sql(
                """
                UPDATE "Lessons"
                SET "CycleWeek" = 1
                WHERE "SpecificDate" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "ScheduleSettings" ("CycleStartDate")
                SELECT date('now', 'weekday 1', '-6 days')
                WHERE NOT EXISTS (SELECT 1 FROM "ScheduleSettings");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleSettings");

            migrationBuilder.DropTable(
                name: "StudentStudyGroups");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_SpecificDate",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsRepeating",
                table: "StudyGroups");

            migrationBuilder.DropColumn(
                name: "CycleWeek",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "SpecificDate",
                table: "Lessons");
        }
    }
}
