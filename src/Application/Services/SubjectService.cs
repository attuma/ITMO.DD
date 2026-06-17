using StudentTracker.Application.DTO;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Services;

// сервис для работы с предметами пользователя
public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<SubjectResponse> CreateAsync(SubjectRequest request, int userId)
    {
        // CreateForUser - XOR правило: предмет принадлежит либо пользователю либо группе
        var subject = Subject.CreateForUser(request.SubjectName, request.Description, userId);

        await _subjectRepository.AddAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        return new SubjectResponse(subject.Id, subject.SubjectName, subject.Description, subject.IsArchived);
    }

    public async Task<List<SubjectResponse>> GetUserSubjectsAsync(int userId)
    {
        var subjects = await _subjectRepository.GetByUserIdAsync(userId);

        // превращаем каждый Subject в SubjectResponse
        return subjects
            .Select(s => new SubjectResponse(s.Id, s.SubjectName, s.Description, s.IsArchived))
            .ToList();
    }
}
