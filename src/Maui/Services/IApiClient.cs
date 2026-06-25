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
    Task<IReadOnlyList<SessionApiResponse>> GetSessionsAsync();

    // старт: при наличии активной сессии вернётся ошибка (409)
    Task<(SessionApiResponse? session, string? error)> StartSessionAsync(int subjectId);

    Task<SessionApiResponse?> PauseSessionAsync(int sessionId);
    Task<SessionApiResponse?> ResumeSessionAsync(int sessionId);
    Task<SessionApiResponse?> CompleteSessionAsync(int sessionId);
}
