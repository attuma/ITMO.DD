namespace itmodd.Models;

public class GroupMemberApiResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string GroupRole { get; set; } = string.Empty;
    public bool IsStudying { get; set; }
    public long TodaySeconds { get; set; }
}
