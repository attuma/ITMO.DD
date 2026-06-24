using StudentTracker.Application.DTO;

namespace StudentTracker.Application.Interfaces;

public interface ISubjectService
{
    Task<SubjectResponse> CreateAsync(SubjectRequest request, int userId);
    Task<List<SubjectResponse>> GetUserSubjectsAsync(int userId);
    Task<SubjectResponse> CreateForGroupAsync(SubjectRequest request, int groupId, int userId);
    Task<List<SubjectResponse>> GetGroupSubjectsAsync(int groupId, int userId);
    Task ArchiveAsync(int subjectId, int userId);
    Task<List<SubjectResponse>> GetAccessibleSubjectsAsync(int userId);
}
// CreateAsync создает предмет для пользователя
// GetUserSubjectsAsync возвращает все предметы пользователя
