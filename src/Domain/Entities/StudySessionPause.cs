using StudentTracker.Domain.Common;

namespace StudentTracker.Domain.Entities;

public class StudySessionPause : BaseEntity
{
    public int SessionId { get; private set; }
    public DateTime PausedAt { get; private set; }
    public DateTime? ResumedAt { get; private set; }

    private StudySessionPause()
    {
        PausedAt = DateTime.UtcNow;
    }

    public StudySessionPause(int sessionId, DateTime? pausedAt = null)
    {
        if (sessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(sessionId));

        SessionId = sessionId;
        PausedAt = pausedAt ?? DateTime.UtcNow;
    }

    public void Resume(DateTime? resumedAt = null)
    {
        var resumeTime = resumedAt ?? DateTime.UtcNow;

        if (resumeTime < PausedAt)
            throw new ArgumentException("Resume time cannot be earlier than pause time.", nameof(resumedAt));

        ResumedAt = resumeTime;
    }
}
