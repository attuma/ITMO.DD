using StudentTracker.Application.DTO;

namespace StudentTracker.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(TaskRequest request, int userId);
    Task<List<TaskResponse>> GetUserTasksAsync(int userId);
}
// CreateAsync создаёт личную задачу (предмет должен принадлежать пользователю)
// GetUserTasksAsync возвращает все задачи пользователя
