using StudentTracker.Application.DTO;

namespace StudentTracker.Application.Interfaces;

public interface ILeaderboardService
{
    Task<List<LeaderboardEntryResponse>> GetDailyLeaderboardAsync();
    Task<List<LeaderboardEntryResponse>> GetWeeklyLeaderboardAsync();
    Task<List<LeaderboardEntryResponse>> GetMonthlyLeaderboardAsync();
}
