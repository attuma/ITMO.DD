using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Infrastructure.Persistence;

namespace StudentTracker.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    // все задачи пользователя
    public async Task<List<TaskItem>> GetByUserIdAsync(int userId)
    {
        return await _db.TaskItems
            .Where(t => t.OwnerUserId == userId)
            .ToListAsync();
    }

    // одна задача по id (или null)
    public async Task<TaskItem?> GetByIdAsync(int taskId)
    {
        return await _db.TaskItems
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    // запомнить новую задачу
    public async Task AddAsync(TaskItem task)
    {
        await _db.TaskItems.AddAsync(task);
    }

    // отправить изменения в базу
    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}