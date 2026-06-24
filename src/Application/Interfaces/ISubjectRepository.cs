using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetByUserIdAsync(int userId);
    Task<List<Subject>> GetByGroupIdAsync(int groupId);
    Task<Subject?> GetByIdAsync(int subjectId);
    Task AddAsync(Subject subject);
    Task SaveChangesAsync();
}
// Реализация будет находится в Infrastructure

// GetByUserIdAsync - SELECT * FROM subjects WHERE owner_user_id = userId
// GetByIdAsync - SELECT * FROM subjects WHERE id = subjectId
// AddAsync добавляет новый предмет
// SaveChangesAsync отправляет все изменения в БД
