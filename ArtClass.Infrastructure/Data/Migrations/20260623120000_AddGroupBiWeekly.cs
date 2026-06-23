using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtClass.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddGroupBiWeekly : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsBiWeekly",
            table: "StudyGroups",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsBiWeekly",
            table: "StudyGroups");
    }
}
