using StudentTracker.Domain.Entities;


namespace StudentTracker.Application.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(int id);
    Task<List<Group>> GetByUserIdAsync(int userId);
    Task<List<Group>> GetGroupsByMemberAsync(int userId);
    Task AddAsync(Group group);
    Task SaveChangesAsync();


}