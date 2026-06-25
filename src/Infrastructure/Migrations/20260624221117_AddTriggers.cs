using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Триггер 1: нельзя начать сессию если уже есть активная или на паузе
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION check_active_session()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM study_sessions
                        WHERE user_id = NEW.user_id
                          AND session_status IN ('active', 'paused')
                    ) THEN
                        RAISE EXCEPTION 'User already has an active session';
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_prevent_duplicate_active_session
                BEFORE INSERT ON study_sessions
                FOR EACH ROW EXECUTE FUNCTION check_active_session();
            ");

            // Триггер 2: проверка доступа к предмету перед созданием сессии
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION check_subject_access()
                RETURNS TRIGGER AS $$
                DECLARE
                    v_owner_user_id  INTEGER;
                    v_owner_group_id INTEGER;
                BEGIN
                    SELECT owner_user_id, owner_group_id
                    INTO v_owner_user_id, v_owner_group_id
                    FROM subjects
                    WHERE subject_id = NEW.subject_id;

                    IF v_owner_user_id IS NOT NULL AND v_owner_user_id != NEW.user_id THEN
                        RAISE EXCEPTION 'Access denied: subject belongs to another user';
                    END IF;

                    IF v_owner_group_id IS NOT NULL THEN
                        IF NOT EXISTS (
                            SELECT 1 FROM group_memberships
                            WHERE group_id = v_owner_group_id
                              AND user_id  = NEW.user_id
                              AND left_at  IS NULL
                        ) THEN
                            RAISE EXCEPTION 'Access denied: user is not a member of the group';
                        END IF;
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_check_subject_access
                BEFORE INSERT ON study_sessions
                FOR EACH ROW EXECUTE FUNCTION check_subject_access();
            ");

            // Триггер 3: автоматически добавлять создателя группы как GroupOwner
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION add_group_owner_membership()
                RETURNS TRIGGER AS $$
                BEGIN
                    INSERT INTO group_memberships(group_id, user_id, group_role, joined_at)
                    VALUES (NEW.group_id, NEW.owner_user_id, 'group_owner', NOW());
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_add_group_owner
                AFTER INSERT ON groups
                FOR EACH ROW EXECUTE FUNCTION add_group_owner_membership();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_prevent_duplicate_active_session ON study_sessions;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS check_active_session();");

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_check_subject_access ON study_sessions;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS check_subject_access();");

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_add_group_owner ON groups;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS add_group_owner_membership();");
        }
    }
}
