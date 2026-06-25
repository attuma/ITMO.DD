using StudentTracker.Domain.Common;

namespace StudentTracker.Domain.Entities;

public class Subject : BaseEntity
{
    // цвет по умолчанию, если не задан явно (серый)
    public const string DefaultColor = "#808080";

    public string SubjectName { get; private set; }
    public string? Description { get; private set; }
    public int? OwnerUserId { get; private set; }
    public int? OwnerGroupId { get; private set; }
    public bool IsArchived { get; private set; }
    public bool IsDefault { get; private set; }
    public string Color { get; private set; }

    private Subject()
    {
        SubjectName = string.Empty;
        Color = DefaultColor;
    }

    public Subject(
        string subjectName,
        string? description,
        int? ownerUserId,
        int? ownerGroupId,
        bool isDefault = false,
        string? color = null)
    {
        if (string.IsNullOrWhiteSpace(subjectName))
            throw new ArgumentException("Subject name cannot be empty.", nameof(subjectName));

        ValidateOwner(ownerUserId, ownerGroupId);

        SubjectName = subjectName;
        Description = description;
        OwnerUserId = ownerUserId;
        OwnerGroupId = ownerGroupId;
        IsDefault = isDefault;
        IsArchived = false;
        Color = string.IsNullOrWhiteSpace(color) ? DefaultColor : color;
    }

    public static Subject CreateForUser(string subjectName, string? description, int ownerUserId, bool isDefault = false, string? color = null)
    {
        if (ownerUserId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerUserId));

        return new Subject(subjectName, description, ownerUserId, null, isDefault, color);
    }

    public static Subject CreateForGroup(string subjectName, string? description, int ownerGroupId, string? color = null)
    {
        if (ownerGroupId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerGroupId));

        return new Subject(subjectName, description, null, ownerGroupId, false, color);
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Subject name cannot be empty.", nameof(newName));

        SubjectName = newName;
    }

    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription;
    }

    private static void ValidateOwner(int? ownerUserId, int? ownerGroupId)
    {
        var hasUserOwner = ownerUserId.HasValue;
        var hasGroupOwner = ownerGroupId.HasValue;

        if (hasUserOwner == hasGroupOwner)
            throw new ArgumentException("Subject must have either user owner or group owner.");

        if (ownerUserId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerUserId));

        if (ownerGroupId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerGroupId));
    }
}
