using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Infrastructure.Persistence;
using System.Data;

namespace StudentTracker.Infrastructure.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly AppDbContext _db;

    public SubjectRepository(AppDbContext db)
    {
        _db = db;
    }

    // SELECT * FROM subjects WHERE owner_user_id = userId
    public async Task<List<Subject>> GetByUserIdAsync(int userId)
    {
        return await _db.Subjects.Where(s => s.OwnerUserId == userId).ToListAsync();
    }

    // SELECT * FROM subjects WHERE owner_group_id = groupId
    public async Task<List<Subject>> GetByGroupIdAsync(int groupId)
    {
        return await _db.Subjects.Where(s => s.OwnerGroupId == groupId).ToListAsync();
    }

    // SELECT * FROM subjects WHERE id = subjectId LIMIT 1
    public async Task<Subject?> GetByIdAsync(int subjectId)
    {
        return await _db.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
    }

    // читаем из View user_accessible_subjects — личные предметы и предметы групп пользователя
    public async Task<List<Subject>> GetAccessibleByUserIdAsync(int userId)
    {
        return await _db.Subjects
            .FromSqlRaw(@"
                SELECT DISTINCT s.*
                FROM user_accessible_subjects s
                WHERE s.owner_user_id = {0}
                   OR s.accessible_by_user_id = {0}
            ", userId)
            .ToListAsync();
    }

    public async Task AddAsync(Subject subject)
    {
        await _db.Subjects.AddAsync(subject);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
