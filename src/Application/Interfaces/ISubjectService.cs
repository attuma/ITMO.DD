using StudentTracker.Application.DTO;

namespace StudentTracker.Application.Interfaces;

public interface ISubjectService
{
    Task<SubjectResponse> CreateAsync(SubjectRequest request, int userId);
    Task<List<SubjectResponse>> GetUserSubjectsAsync(int userId);
}
// CreateAsync создает предмет для пользователя
// GetUserSubjectsAsync возвращает все предметы пользователя
