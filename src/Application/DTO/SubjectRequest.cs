namespace StudentTracker.Application.DTO;

// просто форма данных (Color — необязательный hex-цвет, напр. "#FF8C00")
public record SubjectRequest(string SubjectName, string? Description, string? Color = null);
