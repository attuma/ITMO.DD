using StudentTracker.Domain.Common;

namespace StudentTracker.Domain.Entities;

public class Group : BaseEntity
{
    public string GroupName { get; private set; }
    public string? Description { get; private set; }
    public bool IsArchived { get; private set; }
    public int OwnerUserId { get; private set; }
    public string JoinCode { get; private set; }

    private Group()
    {
        GroupName = string.Empty;
        JoinCode = string.Empty;
    }

    public Group(string groupName, string? description, int ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentException("Group name cannot be empty.", nameof(groupName));

        if (ownerUserId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ownerUserId));

        GroupName = groupName;
        Description = description;
        OwnerUserId = ownerUserId;
        IsArchived = false;
        JoinCode = Guid.NewGuid().ToString("N")[..8].ToUpper();
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Group name cannot be empty.", nameof(newName));

        GroupName = newName;
    }

    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription;
    }
}
