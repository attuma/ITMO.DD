namespace itmodd.Models;

/// <summary>
/// Учебная сессия в том виде, как её отдаёт API (<c>/sessions</c>).
/// Зеркало серверного <c>SessionResponse</c>. <see cref="Status"/> — строка из
/// серверного enum (<c>StudySessionStatus.ToString()</c>): Active / Paused / Completed / Cancelled.
/// </summary>
public class SessionApiResponse
{
    /// <summary>Id сессии — нужен для pause/resume/complete.</summary>
    public int Id { get; set; }

    /// <summary>Предмет, по которому идёт сессия.</summary>
    public int SubjectId { get; set; }

    /// <summary>Когда начали (UTC). От неё считается прошедшее время.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Когда завершили; null — пока сессия не закончена. Заполнено = попадает в историю.</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Статус строкой (Active/Paused/Completed/Cancelled).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Идёт прямо сейчас (счётчик тикает).</summary>
    public bool IsActive => Status == "Active";

    /// <summary>На паузе (счётчик стоит, но сессия не завершена).</summary>
    public bool IsPaused => Status == "Paused";
}
