using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "subjects",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "#808080");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                table: "subjects");
        }
    }
}
