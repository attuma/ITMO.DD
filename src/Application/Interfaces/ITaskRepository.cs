namespace StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetByUserIdAsync(int userId);
    Task<TaskItem?> GetByIdAsync(int taskId);
    Task AddAsync(TaskItem task);
    Task SaveChangesAsync();
}