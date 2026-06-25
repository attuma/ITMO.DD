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
}
