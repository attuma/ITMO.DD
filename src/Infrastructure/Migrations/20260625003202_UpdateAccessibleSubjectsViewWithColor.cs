using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccessibleSubjectsViewWithColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // пересоздаём view со всеми колонками subjects (включая новый color),
            // иначе SELECT s.* -> сущность Subject падает на нехватке колонок
            migrationBuilder.Sql("DROP VIEW IF EXISTS user_accessible_subjects;");
            migrationBuilder.Sql(@"
                CREATE VIEW user_accessible_subjects AS
                SELECT
                    s.subject_id,
                    s.subject_name,
                    s.description,
                    s.is_archived,
                    s.is_default,
                    s.color,
                    s.created_at,
                    s.owner_user_id,
                    s.owner_group_id,
                    gm.user_id AS accessible_by_user_id
                FROM subjects s
                LEFT JOIN group_memberships gm
                    ON gm.group_id = s.owner_group_id
                    AND gm.left_at IS NULL
                WHERE s.is_archived = false;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // возвращаем исходную версию view (без новых колонок)
            migrationBuilder.Sql("DROP VIEW IF EXISTS user_accessible_subjects;");
            migrationBuilder.Sql(@"
                CREATE VIEW user_accessible_subjects AS
                SELECT
                    s.subject_id,
                    s.subject_name,
                    s.description,
                    s.is_archived,
                    s.owner_user_id,
                    s.owner_group_id,
                    gm.user_id AS accessible_by_user_id
                FROM subjects s
                LEFT JOIN group_memberships gm
                    ON gm.group_id = s.owner_group_id
                    AND gm.left_at IS NULL
                WHERE s.is_archived = false;
            ");
        }
    }
}
