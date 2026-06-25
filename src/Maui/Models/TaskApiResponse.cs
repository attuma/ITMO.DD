namespace itmodd.Models;

// Форма задачи, как её отдаёт API (GET /tasks). Маппится в DeadlineItem.
// Status — TaskProgressStatus с сервера: 1=NotStarted, 2=InProgress, 3=Completed, 4=Dismissed.
public class TaskApiResponse
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SubjectId { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public bool IsArchived { get; set; }
    public string Color { get; set; } = "#808080";
    public int Status { get; set; }

    // 3 = Completed
    public bool IsDone => Status == 3;
}
