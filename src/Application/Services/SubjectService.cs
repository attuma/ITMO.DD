using StudentTracker.Application.DTO;
using StudentTracker.Application.Exceptions;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Services;

// сервис для работы с предметами пользователя
public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IGroupMembershipRepository _groupMembershipRepository;

    public SubjectService(ISubjectRepository subjectRepository, IGroupMembershipRepository groupMembershipRepository)
    {
        _subjectRepository = subjectRepository;
        _groupMembershipRepository = groupMembershipRepository;
    }

    public async Task<SubjectResponse> CreateAsync(SubjectRequest request, int userId)
    {
        // CreateForUser - XOR правило: предмет принадлежит либо пользователю либо группе
        var subject = Subject.CreateForUser(request.SubjectName, request.Description, userId, color: request.Color);

        await _subjectRepository.AddAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        return Map(subject);
    }

    public async Task<List<SubjectResponse>> GetUserSubjectsAsync(int userId)
    {
        var subjects = await _subjectRepository.GetByUserIdAsync(userId);

        // превращаем каждый Subject в SubjectResponse
        return subjects.Select(Map).ToList();
    }

    // CreateForGroupAsync — создаёт предмет для группы, только для участников группы
    public async Task<SubjectResponse> CreateForGroupAsync(SubjectRequest request, int groupId, int userId)
    {
        var membership = await _groupMembershipRepository.GetByUserAndGroupAsync(userId, groupId);
        if (membership == null) throw new ForbiddenException("Access denied");

        var subject = Subject.CreateForGroup(request.SubjectName, request.Description, groupId, request.Color);
        await _subjectRepository.AddAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        return Map(subject);
    }

    // GetGroupSubjectsAsync — возвращает все предметы группы, только для участников
    public async Task<List<SubjectResponse>> GetGroupSubjectsAsync(int groupId, int userId)
    {
        var membership = await _groupMembershipRepository.GetByUserAndGroupAsync(userId, groupId);
        if (membership == null) throw new ForbiddenException("Access denied");

        var subjects = await _subjectRepository.GetByGroupIdAsync(groupId);
        return subjects.Select(Map).ToList();
    }

    private static SubjectResponse Map(Subject s) =>
        new(s.Id, s.SubjectName, s.Description, s.IsArchived, s.Color);
}
