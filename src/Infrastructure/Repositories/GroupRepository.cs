using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Infrastructure.Persistence;

namespace StudentTracker.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _db;

    public GroupRepository(AppDbContext db)
    {
        _db = db;
    }

    // SELECT * FROM groups WHERE id = groupId LIMIT 1
    public async Task<Group?> GetByIdAsync(int groupId)
        => await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

    // SELECT * FROM groups WHERE join_code = joinCode LIMIT 1
    public async Task<Group?> GetByJoinCodeAsync(string joinCode)
        => await _db.Groups.FirstOrDefaultAsync(g => g.JoinCode == joinCode);

    // SELECT * FROM groups WHERE owner_user_id = userId
    public async Task<List<Group>> GetByUserIdAsync(int userId)
        => await _db.Groups.Where(g => g.OwnerUserId == userId).ToListAsync();

    // все активные (не архивированные) группы где пользователь является участником
    public async Task<List<Group>> GetGroupsByMemberAsync(int userId)
        => await _db.GroupMemberships
            .Where(m => m.UserId == userId && m.LeftAt == null)
            .Join(_db.Groups, m => m.GroupId, g => g.Id, (m, g) => g)
            .Where(g => !g.IsArchived)
            .ToListAsync();

    // AddAsync: запоминает группу в EF Core, SQL ещё не выполняется
    public async Task AddAsync(Group group)
        => await _db.Groups.AddAsync(group);

    // SaveChangesAsync: отправляет всё в БД одной транзакцией — BEGIN → INSERT → COMMIT
    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
