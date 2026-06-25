using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using itmodd.Models;

namespace itmodd.Services;

// Реальные данные календаря: тянет задачи из API (GET /tasks).
// Падать не должен: нет токена / API недоступен -> возвращаем пусто.
public class ApiCalendarDataService : ICalendarDataService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiCalendarDataService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<DeadlineItem>> GetDeadlinesAsync(int year, int month)
    {
        var tasks = await FetchTasksAsync();

        // только задачи этого месяца с дедлайном, не архивные
        return tasks
            .Where(t => !t.IsArchived && t.DeadlineAt is { } d && d.Year == year && d.Month == month)
            .Select(ToDeadlineItem)
            .ToList();
    }

    public async Task<DeadlineSummary> GetSummaryAsync()
    {
        var tasks = (await FetchTasksAsync()).Where(t => !t.IsArchived).ToList();
        var today = DateTime.Today;

        // понедельник текущей недели ... воскресенье (включительно)
        int offset = ((int)today.DayOfWeek + 6) % 7;
        var weekStart = today.AddDays(-offset);
        var weekEnd = weekStart.AddDays(7);

        // дедлайн как дата (если он есть)
        static DateTime? Day(TaskApiResponse t) => t.DeadlineAt?.Date;

        return new DeadlineSummary(
            Today:   tasks.Count(t => !t.IsDone && Day(t) == today),
            Week:    tasks.Count(t => !t.IsDone && Day(t) is { } d && d >= weekStart && d < weekEnd),
            Overdue: tasks.Count(t => !t.IsDone && t.DeadlineAt is { } dl && dl < DateTime.Now),
            Done:    tasks.Count(t => t.IsDone));
    }

    // один поход в API; ошибки гасим и отдаём пустой список
    private async Task<List<TaskApiResponse>> FetchTasksAsync()
    {
        var token = ApiConfig.Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            Debug.WriteLine("[ApiCalendarDataService] нет JWT-токена — данные не загружаются");
            return new List<TaskApiResponse>();
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/tasks");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[ApiCalendarDataService] GET /tasks -> {(int)response.StatusCode}");
                return new List<TaskApiResponse>();
            }

            var tasks = await response.Content.ReadFromJsonAsync<List<TaskApiResponse>>(JsonOptions);
            return tasks ?? new List<TaskApiResponse>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiCalendarDataService] ошибка запроса: {ex.Message}");
            return new List<TaskApiResponse>();
        }
    }

    private static DeadlineItem ToDeadlineItem(TaskApiResponse t) => new()
    {
        Title = t.Title,
        Deadline = t.DeadlineAt!.Value,
        Color = t.Color,
        IsDone = t.IsDone,
    };
}
