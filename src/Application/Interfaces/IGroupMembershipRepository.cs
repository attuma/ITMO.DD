using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Interfaces;

public interface IGroupMembershipRepository
{
    Task<List<GroupMembership>> GetByGroupIdAsync(int groupId);
    Task<GroupMembership?> GetByUserAndGroupAsync(int userId, int groupId);
    Task<GroupMembership?> GetByUserAndGroupIncludingLeftAsync(int userId, int groupId);
    Task AddAsync(GroupMembership membership);
    Task SaveChangesAsync();
}