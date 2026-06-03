using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class TaskProgress
{
    public int TaskId { get; private set; }
    public int UserId { get; private set; }
    public TaskProgressStatus ProgressStatus { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TaskProgress()
    {
        ProgressStatus = TaskProgressStatus.NotStarted;
        CreatedAt = DateTime.UtcNow;
    }

    public TaskProgress(int taskId, int userId)
    {
        if (taskId <= 0)
            throw new ArgumentOutOfRangeException(nameof(taskId));

        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        TaskId = taskId;
        UserId = userId;
        ProgressStatus = TaskProgressStatus.NotStarted;
        CreatedAt = DateTime.UtcNow;
    }

    public void Start(DateTime? startedAt = null)
    {
        StartedAt = startedAt ?? DateTime.UtcNow;
        ProgressStatus = TaskProgressStatus.InProgress;
    }

    public void Complete(DateTime? completedAt = null)
    {
        var completeTime = completedAt ?? DateTime.UtcNow;

        if (StartedAt.HasValue && completeTime < StartedAt.Value)
            throw new ArgumentException("Complete time cannot be earlier than start time.", nameof(completedAt));

        StartedAt ??= completeTime;
        CompletedAt = completeTime;
        ProgressStatus = TaskProgressStatus.Completed;
    }

    public void Dismiss()
    {
        ProgressStatus = TaskProgressStatus.Dismissed;
    }
}
