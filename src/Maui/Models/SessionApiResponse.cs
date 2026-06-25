namespace itmodd.Models;

// учебная сессия, как её отдаёт API (/sessions)
// Status: Active / Paused / Completed / Cancelled
public class SessionApiResponse
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;

    public bool IsActive => Status == "Active";
    public bool IsPaused => Status == "Paused";
}
