namespace StudentTracker.Application.DTO;

public record TaskRequest(string Title, string? Description, int SubjectId, DateTime? DeadlineAt);