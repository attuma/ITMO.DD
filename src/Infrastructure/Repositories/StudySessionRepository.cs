using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Infrastructure.Persistence;

namespace StudentTracker.Infrastructure.Repositories;

public class StudySessionRepository : IStudySessionRepository
{
    private readonly AppDbContext _db;

    public StudySessionRepository(AppDbContext db)
    {
        _db = db;
    }

    // SELECT * FROM study_sessions WHERE id = sessionId LIMIT 1
    public async Task<StudySession?> GetByIdAsync(int sessionId)
        => await _db.StudySessions.FirstOrDefaultAsync(s => s.Id == sessionId);

    // SELECT * FROM study_sessions WHERE user_id = userId
    public async Task<List<StudySession>> GetByUserIdAsync(int userId)
        => await _db.StudySessions.Where(s => s.UserId == userId).ToListAsync();

    public async Task AddAsync(StudySession session)
        => await _db.StudySessions.AddAsync(session);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
