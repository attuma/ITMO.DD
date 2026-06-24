using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class GroupMembership
{
    public int GroupId { get; private set; }
    public int UserId { get; private set; }
    public GroupRole GroupRole { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    private GroupMembership()
    {
        GroupRole = GroupRole.Member;
        JoinedAt = DateTime.UtcNow;
    }

    public GroupMembership(int groupId, int userId, GroupRole groupRole = GroupRole.Member, DateTime? joinedAt = null)
    {
        if (groupId <= 0)
            throw new ArgumentOutOfRangeException(nameof(groupId));

        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        if (!Enum.IsDefined(groupRole))
            throw new ArgumentException("Invalid group role.", nameof(groupRole));

        GroupId = groupId;
        UserId = userId;
        GroupRole = groupRole;
        JoinedAt = joinedAt ?? DateTime.UtcNow;
    }

    public void Leave(DateTime? leftAt = null)
    {
        var leaveTime = leftAt ?? DateTime.UtcNow;

        if (leaveTime < JoinedAt)
            throw new ArgumentException("Leave time cannot be earlier than join time.", nameof(leftAt));

        LeftAt = leaveTime;
    }

    public void Rejoin()
    {
        JoinedAt = DateTime.UtcNow;
        LeftAt = null;
    }
}
