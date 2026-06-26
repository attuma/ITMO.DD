namespace itmodd.Models;

public class GroupApiResponse
{
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
