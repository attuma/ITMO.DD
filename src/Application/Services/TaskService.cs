using StudentTracker.Application.DTO;
using StudentTracker.Application.Exceptions;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;

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

        // новая задача: цвет предмета известен, прогресс ещё не начат
        return Map(task, subject.Color, TaskProgressStatus.NotStarted);
    }

    public async Task<List<TaskResponse>> GetUserTasksAsync(int userId)
    {
        var tasks = await _taskRepository.GetByUserIdAsync(userId);

        // цвет берём из предметов пользователя (SubjectId -> Color)
        var subjects = await _subjectRepository.GetByUserIdAsync(userId);
        var colorBySubject = subjects.ToDictionary(s => s.Id, s => s.Color);

        // статус — из прогресса пользователя (TaskId -> Status); нет записи => NotStarted
        var progress = await _taskRepository.GetProgressByUserAsync(userId);
        var statusByTask = progress.ToDictionary(p => p.TaskId, p => p.ProgressStatus);

        return tasks.Select(t => Map(
            t,
            colorBySubject.GetValueOrDefault(t.SubjectId, Subject.DefaultColor),
            statusByTask.GetValueOrDefault(t.Id, TaskProgressStatus.NotStarted)))
            .ToList();
    }

    private static TaskResponse Map(TaskItem t, string color, TaskProgressStatus status) =>
        new(t.Id, t.Title, t.Description, t.SubjectId, t.DeadlineAt, t.IsArchived, color, status);
}
