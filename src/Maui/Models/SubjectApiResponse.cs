namespace itmodd.Models;

// предмет, как его отдаёт API (GET /subjects, POST /subjects)
public class SubjectApiResponse
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public string Color { get; set; } = "#808080";
}
