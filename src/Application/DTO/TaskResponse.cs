using StudentTracker.Domain.Enums;

namespace StudentTracker.Application.DTO;

// Color — цвет предмета (для точки в календаре), Status — прогресс пользователя по задаче
public record TaskResponse(
    int TaskId,
    string Title,
    string? Description,
    int SubjectId,
    DateTime? DeadlineAt,
    bool IsArchived,
    string Color,
    TaskProgressStatus Status);
