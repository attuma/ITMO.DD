namespace StudentTracker.Application.DTO;

// просто форма данных
public record SubjectResponse(int SubjectId, string SubjectName, string? Description, bool IsArchived, string Color);
