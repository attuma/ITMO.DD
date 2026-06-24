namespace StudentTracker.Application.DTO;

public record TaskResponse(int TaskId, string Title, string? Description, int SubjectId, DateTime? DeadlineAt , bool IsArchived );
