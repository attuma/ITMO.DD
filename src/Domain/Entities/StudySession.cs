using StudentTracker.Domain.Common;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class StudySession : BaseEntity
{
    public int UserId { get; private set; }
    public int SubjectId { get; private set; }
    public int? TaskId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public StudySessionStatus SessionStatus { get; private set; }

    private StudySession()
    {
        SessionStatus = StudySessionStatus.Active;
    }

    public StudySession(int userId, int subjectId, int? taskId = null, DateTime? startedAt = null)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        if (subjectId <= 0)
            throw new ArgumentOutOfRangeException(nameof(subjectId));

        if (taskId <= 0)
            throw new ArgumentOutOfRangeException(nameof(taskId));

        UserId = userId;
        SubjectId = subjectId;
        TaskId = taskId;
        StartedAt = startedAt ?? DateTime.UtcNow;
        SessionStatus = StudySessionStatus.Active;
    }

    public void Pause()
    {
        if (SessionStatus != StudySessionStatus.Active)
            throw new InvalidOperationException("Only active session can be paused.");

        SessionStatus = StudySessionStatus.Paused;
    }

    public void Resume()
    {
        if (SessionStatus != StudySessionStatus.Paused)
            throw new InvalidOperationException("Only paused session can be resumed.");

        SessionStatus = StudySessionStatus.Active;
    }

    public void Complete(DateTime? endedAt = null)
    {
        var endTime = endedAt ?? DateTime.UtcNow;

        if (endTime < StartedAt)
            throw new ArgumentException("Session end time cannot be earlier than start time.", nameof(endedAt));

        EndedAt = endTime;
        SessionStatus = StudySessionStatus.Completed;
    }

    public void Cancel(DateTime? endedAt = null)
    {
        var endTime = endedAt ?? DateTime.UtcNow;

        if (endTime < StartedAt)
            throw new ArgumentException("Session end time cannot be earlier than start time.", nameof(endedAt));

        EndedAt = endTime;
        SessionStatus = StudySessionStatus.Cancelled;
    }
}
