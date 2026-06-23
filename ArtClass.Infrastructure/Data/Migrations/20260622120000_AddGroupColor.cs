using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtClass.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddGroupColor : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Color",
            table: "StudyGroups",
            type: "TEXT",
            maxLength: 9,
            nullable: false,
            defaultValue: "#512BD4");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Color",
            table: "StudyGroups");
    }
}
