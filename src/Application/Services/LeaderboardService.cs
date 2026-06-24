using Microsoft.Extensions.Caching.Distributed;
using StudentTracker.Application.DTO;
using StudentTracker.Application.Interfaces;
using System.Text.Json;

namespace StudentTracker.Application.Services;

// сервис для подсчёта рейтинга пользователей по суммарному времени сессий
public class LeaderboardService : ILeaderboardService
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDistributedCache _cache;

    public LeaderboardService(IStudySessionRepository studySessionRepository, IUserRepository userRepository, IDistributedCache cache)
    {
        _studySessionRepository = studySessionRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public Task<List<LeaderboardEntryResponse>> GetDailyLeaderboardAsync()
    {
        var from = DateTime.UtcNow.Date; // сегодня с 00:00
        return GetLeaderboardAsync("leaderboard:daily", from);
    }

    public Task<List<LeaderboardEntryResponse>> GetWeeklyLeaderboardAsync()
    {
        var from = DateTime.UtcNow.Date.AddDays(-7);
        return GetLeaderboardAsync("leaderboard:weekly", from);
    }

    public Task<List<LeaderboardEntryResponse>> GetMonthlyLeaderboardAsync()
    {
        var from = DateTime.UtcNow.Date.AddDays(-30);
        return GetLeaderboardAsync("leaderboard:monthly", from);
    }

    private async Task<List<LeaderboardEntryResponse>> GetLeaderboardAsync(string cacheKey, DateTime from)
    {
        // проверяем Redis — если данные есть, возвращаем без запроса в БД
        try
        {
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<List<LeaderboardEntryResponse>>(cached)!;
        }
        catch
        {
            // Redis недоступен — fallback в БД, не падаем
        }

        var result = await CalculateFromDbAsync(from);

        // кладём результат в Redis на 5 минут
        try
        {
            var json = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }
        catch
        {
            // Redis недоступен — продолжаем без кэша
        }

        return result;
    }

    private async Task<List<LeaderboardEntryResponse>> CalculateFromDbAsync(DateTime from)
    {
        var sessions = await _studySessionRepository.GetCompletedSinceAsync(from);

        // группируем по UserId и считаем суммарное время в секундах
        var grouped = sessions
            .Where(s => s.EndedAt != null)
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalSeconds = (long)g.Sum(s => (s.EndedAt!.Value - s.StartedAt).TotalSeconds)
            })
            .OrderByDescending(x => x.TotalSeconds)
            .ToList();

        var result = new List<LeaderboardEntryResponse>();
        foreach (var entry in grouped)
        {
            var user = await _userRepository.GetByIdAsync(entry.UserId);
            if (user != null)
                result.Add(new LeaderboardEntryResponse(entry.UserId, user.UserName, entry.TotalSeconds));
        }
        return result;
    }
}
