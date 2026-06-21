using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Infrastructure.Persistence;

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

    // SELECT * FROM subjects WHERE id = subjectId LIMIT 1
    public async Task<Subject?> GetByIdAsync(int subjectId)
    {
        return await _db.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
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
