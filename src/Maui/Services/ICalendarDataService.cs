using itmodd.Models;

namespace itmodd.Services;

// Источник задач для календаря. VM не знает, откуда они берутся —
// сейчас заглушка, позже HTTP к API (task_items по deadline_at, личные + групповые).
public interface ICalendarDataService
{
    // Задачи с дедлайном в указанном месяце.
    Task<IReadOnlyList<DeadlineItem>> GetDeadlinesAsync(int year, int month);

    // Счётчики для верхних карточек: сегодня / на этой неделе / просрочено / выполнено.
    Task<DeadlineSummary> GetSummaryAsync();
}
