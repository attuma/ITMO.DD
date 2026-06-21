using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Interfaces;

public interface IStudySessionRepository
{
    Task<StudySession?> GetByIdAsync(int sessionId);
    Task<List<StudySession>> GetByUserIdAsync(int userId);
    Task AddAsync(StudySession session);
    Task SaveChangesAsync();
}