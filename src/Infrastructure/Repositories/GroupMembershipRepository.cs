using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Infrastructure.Persistence;

namespace StudentTracker.Infrastructure.Repositories;

public class GroupMembershipRepository : IGroupMembershipRepository
{
    private readonly AppDbContext _db;

    public GroupMembershipRepository(AppDbContext db)
    {
        _db = db;
    }

    // только активные участники — у вышедших LeftAt != null
    public async Task<List<GroupMembership>> GetByGroupIdAsync(int groupId)
        => await _db.GroupMemberships.Where(m => m.GroupId == groupId && m.LeftAt == null).ToListAsync();

    // ищем только активное членство — чтобы вышедший мог вступить снова
    public async Task<GroupMembership?> GetByUserAndGroupAsync(int userId, int groupId)
        => await _db.GroupMemberships.FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId && m.LeftAt == null);

    // ищем любую запись включая вышедших — для повторного вступления
    public async Task<GroupMembership?> GetByUserAndGroupIncludingLeftAsync(int userId, int groupId)
        => await _db.GroupMemberships.FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);

    // AddAsync: запоминает участника в EF Core, SQL ещё не выполняется
    public async Task AddAsync(GroupMembership membership)
        => await _db.GroupMemberships.AddAsync(membership);

    // SaveChangesAsync: отправляет всё в БД одной транзакцией — BEGIN → INSERT/UPDATE → COMMIT
    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
