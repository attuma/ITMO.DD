using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Interfaces;

public interface IStudySessionRepository
{
    Task<StudySession?> GetByIdAsync(int sessionId);
    Task<List<StudySession>> GetByUserIdAsync(int userId);
    Task<StudySession?> GetActiveByUserIdAsync(int userId);
    Task<List<StudySession>> GetCompletedSinceAsync(DateTime from);
    Task AddAsync(StudySession session);
    Task SaveChangesAsync();
}