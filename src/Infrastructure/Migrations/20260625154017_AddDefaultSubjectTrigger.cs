using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultSubjectTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION create_default_subject()
                RETURNS TRIGGER AS $$
                BEGIN
                    INSERT INTO subjects(subject_name, description, owner_user_id, owner_group_id, is_archived, is_default, created_at)
                    VALUES ('General', 'Default subject', NEW.user_id, NULL, false, true, NOW());
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_create_default_subject
                AFTER INSERT ON users
                FOR EACH ROW EXECUTE FUNCTION create_default_subject();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_create_default_subject ON users;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS create_default_subject();");
        }
    }
}
