using itmodd.Models;

namespace itmodd.Services;

// Временная заглушка: отдаёт фейковые задачи, чтобы видеть отрисовку календаря.
// Заменим на реальную реализацию (HTTP к API), когда будет готов бэкенд задач.
public class StubCalendarDataService : ICalendarDataService
{
    public Task<IReadOnlyList<DeadlineItem>> GetDeadlinesAsync(int year, int month)
    {
        // все заглушки привязаны к «сегодня», поэтому показываем их только в текущем месяце
        var today = DateTime.Today;
        if (year != today.Year || month != today.Month)
            return Task.FromResult<IReadOnlyList<DeadlineItem>>(Array.Empty<DeadlineItem>());

        return Task.FromResult<IReadOnlyList<DeadlineItem>>(BuildStubTasks());
    }

    public Task<DeadlineSummary> GetSummaryAsync()
    {
        var today = DateTime.Today;
        var tasks = BuildStubTasks();

        // понедельник текущей недели ... воскресенье (включительно)
        int offset = ((int)today.DayOfWeek + 6) % 7;
        var weekStart = today.AddDays(-offset);
        var weekEnd = weekStart.AddDays(7);

        var summary = new DeadlineSummary(
            Today:   tasks.Count(t => !t.IsDone && t.Deadline.Date == today),
            Week:    tasks.Count(t => !t.IsDone && t.Deadline.Date >= weekStart && t.Deadline.Date < weekEnd),
            Overdue: tasks.Count(t => t.IsOverdue),
            Done:    tasks.Count(t => t.IsDone));

        return Task.FromResult(summary);
    }

    // общий набор фейковых задач — используется и сеткой, и счётчиками
    private static List<DeadlineItem> BuildStubTasks()
    {
        var today = DateTime.Today;
        return new List<DeadlineItem>
        {
            new() { Title = "Лаба по сетям",       Deadline = today.AddHours(23), Color = "#FF8C00" },
            new() { Title = "Домашка по матстату", Deadline = today.AddDays(2),   Color = "#1E90FF" },
            new() { Title = "Курсовая работа",     Deadline = today.AddDays(6),   Color = "#FF4500" },
            // выполненная — ✓ и зачёркнутая
            new() { Title = "Реферат по истории",  Deadline = today.AddDays(1),   Color = "#9370DB", IsDone = true },
            // просроченная — красная
            new() { Title = "Отчёт по практике",   Deadline = today.AddDays(-2),  Color = "#FF4500" },
            // три задачи в один день — покажут бейдж «+N задача»
            new() { Title = "Семинар",             Deadline = today.AddHours(20), Color = "#2E8B57" },
            new() { Title = "Доклад",              Deadline = today.AddHours(21), Color = "#DAA520" },
        };
    }
}
