using StudentTracker.Domain.Common;

namespace StudentTracker.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public int SubjectId { get; private set; }
    public int? OwnerUserId { get; private set; }
    public int? OwnerGroupId { get; private set; }
    public DateTime? DeadlineAt { get; private set; }
    public string? TaskLink { get; private set; }
    public int? MaxPoints { get; private set; }
    public bool IsArchived { get; private set; }

    private TaskItem()
    {
        Title = string.Empty;
    }

    public TaskItem(
        string title,
        string? description,
        int subjectId,
        int? ownerUserId,
        int? ownerGroupId,
        DateTime? deadlineAt = null,
        string? taskLink = null,
        int? maxPoints = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty.", nameof(title));

        if (subjectId <= 0)
            throw new ArgumentOutOfRangeException(nameof(subjectId));

        ValidateOwner(ownerUserId, ownerGroupId);
        ValidateMaxPoints(maxPoints);

        Title = title;
        Description = description;
        SubjectId = subjectId;
        OwnerUserId = ownerUserId;
        OwnerGroupId = ownerGroupId;
        DeadlineAt = deadlineAt;
        TaskLink = taskLink;
        MaxPoints = maxPoints;
        IsArchived = false;
    }

    public static TaskItem CreateForUser(
        string title,
        string? description,
        int subjectId,
        int ownerUserId,
        DateTime? deadlineAt = null,
        string? taskLink = null,
        int? maxPoints = null)
    {
        if (ownerUserId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerUserId));

        return new TaskItem(title, description, subjectId, ownerUserId, null, deadlineAt, taskLink, maxPoints);
    }

    public static TaskItem CreateForGroup(
        string title,
        string? description,
        int subjectId,
        int ownerGroupId,
        DateTime? deadlineAt = null,
        string? taskLink = null,
        int? maxPoints = null)
    {
        if (ownerGroupId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerGroupId));

        return new TaskItem(title, description, subjectId, null, ownerGroupId, deadlineAt, taskLink, maxPoints);
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public void Rename(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Task title cannot be empty.", nameof(newTitle));

        Title = newTitle;
    }

    public void UpdateDetails(string? description, DateTime? deadlineAt, string? taskLink, int? maxPoints)
    {
        ValidateMaxPoints(maxPoints);

        Description = description;
        DeadlineAt = deadlineAt;
        TaskLink = taskLink;
        MaxPoints = maxPoints;
    }

    private static void ValidateOwner(int? ownerUserId, int? ownerGroupId)
    {
        var hasUserOwner = ownerUserId.HasValue;
        var hasGroupOwner = ownerGroupId.HasValue;

        if (hasUserOwner == hasGroupOwner)
            throw new ArgumentException("Task must have either user owner or group owner.");

        if (ownerUserId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerUserId));

        if (ownerGroupId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerGroupId));
    }

    private static void ValidateMaxPoints(int? maxPoints)
    {
        if (maxPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(maxPoints));
    }
}
