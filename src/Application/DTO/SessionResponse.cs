namespace StudentTracker.Application.DTO;

public record SessionResponse(int Id, int SubjectId, DateTime StartedAt, DateTime? EndedAt, string Status);