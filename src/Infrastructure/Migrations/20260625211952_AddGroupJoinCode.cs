using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupJoinCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "join_code",
                table: "groups",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_groups_join_code",
                table: "groups",
                column: "join_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_groups_join_code",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "join_code",
                table: "groups");
        }
    }
}
