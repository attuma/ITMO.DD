using itmodd.Models;

namespace itmodd.Services;

// Клиент для операций записи/справочников (предметы, создание задач).
// Чтение календаря — отдельно в ICalendarDataService.
public interface IApiClient
{
    // предметы пользователя (для выбора при создании задачи)
    Task<IReadOnlyList<SubjectApiResponse>> GetSubjectsAsync();

    // создать предмет; null = не удалось
    Task<SubjectApiResponse?> CreateSubjectAsync(string name, string color);

    // создать задачу; null = успех, иначе текст ошибки
    Task<string?> CreateTaskAsync(string title, string? description, int subjectId, DateTime deadline);

    // ===== учебные сессии (таймер) =====

    /// <summary>Все сессии пользователя — для истории и восстановления активной сессии.</summary>
    Task<IReadOnlyList<SessionApiResponse>> GetSessionsAsync();

    /// <summary>Старт сессии по предмету. При уже активной сессии сервер вернёт 409 → текст в error.</summary>
    Task<(SessionApiResponse? session, string? error)> StartSessionAsync(int subjectId);

    /// <summary>Поставить сессию на паузу. null = не удалось.</summary>
    Task<SessionApiResponse?> PauseSessionAsync(int sessionId);

    /// <summary>Продолжить сессию после паузы. null = не удалось.</summary>
    Task<SessionApiResponse?> ResumeSessionAsync(int sessionId);

    /// <summary>Завершить сессию (фиксирует EndedAt, попадает в историю). null = не удалось.</summary>
    Task<SessionApiResponse?> CompleteSessionAsync(int sessionId);

    // ===== рейтинг =====

    /// <summary>Топ пользователей за период. <paramref name="period"/>: daily | weekly | monthly.</summary>
    Task<IReadOnlyList<LeaderboardEntryResponse>> GetLeaderboardAsync(string period);

    // ===== группы =====

    /// <summary>Список групп пользователя.</summary>
    Task<IReadOnlyList<GroupApiResponse>> GetGroupsAsync();

    /// <summary>Создать группу. null = успех, иначе текст ошибки.</summary>
    Task<(GroupApiResponse? group, string? error)> CreateGroupAsync(string groupName);

    /// <summary>Вступить в группу по коду. null = успех, иначе текст ошибки.</summary>
    Task<(GroupApiResponse? group, string? error)> JoinGroupAsync(string joinCode);

    /// <summary>Участники группы с признаком «учится сейчас» и временем за сегодня.</summary>
    Task<IReadOnlyList<GroupMemberApiResponse>> GetGroupMembersAsync(int groupId);

    /// <summary>Архивировать группу (только лидер). true = успех.</summary>
    Task<bool> ArchiveGroupAsync(int groupId);
}
