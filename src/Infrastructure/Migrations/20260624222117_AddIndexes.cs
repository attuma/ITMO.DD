using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // логин по email — самая частая операция
            migrationBuilder.Sql("CREATE INDEX idx_users_email ON users(email);");

            // GET /sessions — все сессии пользователя
            migrationBuilder.Sql("CREATE INDEX idx_sessions_user_id ON study_sessions(user_id);");

            // лидерборд — фильтр завершённых сессий за период
            migrationBuilder.Sql("CREATE INDEX idx_sessions_status_started ON study_sessions(session_status, started_at);");

            // GET /groups — группы пользователя через memberships
            migrationBuilder.Sql("CREATE INDEX idx_memberships_user_id ON group_memberships(user_id);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_users_email;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_sessions_user_id;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_sessions_status_started;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_memberships_user_id;");
        }
    }
}
