namespace itmodd.Models;

/// <summary>
/// Одна строка рейтинга в том виде, в каком её отдаёт API
/// (<c>GET /leaderboard/daily|weekly|monthly</c>).
/// Зеркало серверного DTO <c>LeaderboardEntryResponse</c> из слоя Application —
/// имена полей должны совпадать, иначе JSON не десериализуется.
/// </summary>
public class LeaderboardEntryResponse
{
    /// <summary>Id пользователя. Нужен, чтобы подсветить в списке строку текущего юзера.</summary>
    public int UserId { get; set; }

    /// <summary>Отображаемое имя пользователя.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Суммарное время учёбы за период в секундах. Форматируется в «ч/мин» на клиенте.</summary>
    public long TotalSeconds { get; set; }
}
