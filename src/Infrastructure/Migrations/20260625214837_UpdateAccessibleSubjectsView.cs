using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccessibleSubjectsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS user_accessible_subjects;");
            migrationBuilder.Sql(@"
                CREATE VIEW user_accessible_subjects AS
                SELECT s.subject_id, s.subject_name, s.description, s.color, s.is_archived,
                       s.owner_user_id, s.owner_group_id,
                       gm.user_id AS accessible_by_user_id
                FROM subjects s
                LEFT JOIN group_memberships gm ON gm.group_id = s.owner_group_id AND gm.left_at IS NULL
                LEFT JOIN groups g ON g.group_id = s.owner_group_id
                WHERE s.is_archived = false
                  AND (s.owner_group_id IS NULL OR g.is_archived = false);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS user_accessible_subjects;");
            migrationBuilder.Sql(@"
                CREATE VIEW user_accessible_subjects AS
                SELECT s.subject_id, s.subject_name, s.description, s.color, s.is_archived,
                       s.owner_user_id, s.owner_group_id,
                       gm.user_id AS accessible_by_user_id
                FROM subjects s
                LEFT JOIN group_memberships gm ON gm.group_id = s.owner_group_id AND gm.left_at IS NULL
                WHERE s.is_archived = false;
            ");
        }
    }
}
