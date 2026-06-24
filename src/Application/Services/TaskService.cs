using StudentTracker.Application.DTO;
using StudentTracker.Application.Exceptions;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Services;

// сервис для работы с личными задачами пользователя
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ISubjectRepository _subjectRepository;

    public TaskService(ITaskRepository taskRepository, ISubjectRepository subjectRepository)
    {
        _taskRepository = taskRepository;
        _subjectRepository = subjectRepository;
    }

    public async Task<TaskResponse> CreateAsync(TaskRequest request, int userId)
    {
        // предмет должен существовать и принадлежать этому пользователю
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId)
            ?? throw new NotFoundException("Subject not found");

        if (subject.OwnerUserId != userId)
            throw new ForbiddenException("Access denied");

        // CreateForUser - XOR правило: задача принадлежит либо пользователю либо группе
        var task = TaskItem.CreateForUser(
            request.Title,
            request.Description,
            request.SubjectId,
            userId,
            request.DeadlineAt);

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return Map(task);
    }

    public async Task<List<TaskResponse>> GetUserTasksAsync(int userId)
    {
        var tasks = await _taskRepository.GetByUserIdAsync(userId);

        // превращаем каждый TaskItem в TaskResponse
        return tasks.Select(Map).ToList();
    }

    private static TaskResponse Map(TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.SubjectId, t.DeadlineAt, t.IsArchived);
}
