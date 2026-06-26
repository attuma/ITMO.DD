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

    // SELECT * FROM study_sessions WHERE user_id = userId ORDER BY started_at DESC
    public async Task<List<StudySession>> GetByUserIdAsync(int userId)
        => await _db.StudySessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();

    // SELECT * FROM study_sessions WHERE user_id = userId AND session_status IN ('active','paused') LIMIT 1
    public async Task<StudySession?> GetActiveByUserIdAsync(int userId)
        => await _db.StudySessions.FirstOrDefaultAsync(s =>
            s.UserId == userId &&
            (s.SessionStatus == Domain.Enums.StudySessionStatus.Active ||
             s.SessionStatus == Domain.Enums.StudySessionStatus.Paused));

    // SELECT * FROM study_sessions WHERE session_status = 'completed' AND started_at >= from
    public async Task<List<StudySession>> GetCompletedSinceAsync(DateTime from)
        => await _db.StudySessions
            .Where(s => s.SessionStatus == Domain.Enums.StudySessionStatus.Completed && s.StartedAt >= from)
            .ToListAsync();

    // сумма секунд учёбы за сегодня (UTC): завершённые + текущая активная
    public async Task<long> GetTodaySecondsAsync(int userId)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var sessions = await _db.StudySessions
            .Where(s => s.UserId == userId && s.StartedAt >= todayUtc)
            .ToListAsync();

        var now = DateTime.UtcNow;
        long total = 0;
        foreach (var s in sessions)
        {
            var end = s.EndedAt ?? now;
            var duration = end - s.StartedAt;
            if (duration.TotalSeconds > 0)
                total += (long)duration.TotalSeconds;
        }
        return total;
    }

    public async Task AddAsync(StudySession session)
        => await _db.StudySessions.AddAsync(session);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
