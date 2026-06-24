using Microsoft.Extensions.Caching.Distributed;
using StudentTracker.Application.DTO;
using StudentTracker.Application.Exceptions;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;


namespace StudentTracker.Application.Services;

// сервис для работы с учебными сессиями таймер
public class StudySessionService : IStudySessionService
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IGroupMembershipRepository _groupMembershipRepository;
    private readonly IDistributedCache _cache;

    public StudySessionService(IStudySessionRepository studySessionRepository, ISubjectRepository subjectRepository, IGroupMembershipRepository groupMembershipRepository, IDistributedCache cache)
    {
        _studySessionRepository = studySessionRepository;
        _subjectRepository = subjectRepository;
        _groupMembershipRepository = groupMembershipRepository;
        _cache = cache;
    }

    // StartAsync — создаёт новую сессию со статусом Active
    public async Task<SessionResponse> StartAsync(StartSessionRequest request, int userId)
    {
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId);
        if (subject == null)
            throw new NotFoundException("Subject not found");

        // личный предмет — только владелец может начать сессию
        if (subject.OwnerUserId != null && subject.OwnerUserId != userId)
            throw new ForbiddenException("Access denied");

        // групповой предмет — проверяем что пользователь состоит в этой группе
        if (subject.OwnerGroupId != null)
        {
            var membership = await _groupMembershipRepository.GetByUserAndGroupAsync(userId, subject.OwnerGroupId.Value);
            if (membership == null)
                throw new ForbiddenException("Access denied");
        }

        // нельзя начать новую сессию если уже есть активная или на паузе
        var activeSession = await _studySessionRepository.GetActiveByUserIdAsync(userId);
        if (activeSession != null)
            throw new ConflictException("You already have an active session");

        var session = new StudySession(userId, request.SubjectId);

        await _studySessionRepository.AddAsync(session);
        await _studySessionRepository.SaveChangesAsync();

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // PauseAsync — ставит сессию на паузу, проверяет что сессия принадлежит пользователю
    public async Task<SessionResponse> PauseAsync(int sessionId, int userId)
    {
        var session = await _studySessionRepository.GetByIdAsync(sessionId);

        if (session == null) throw new NotFoundException("Session not found");
        if (session.UserId != userId) throw new ForbiddenException("Access denied");

        session.Pause();
        await _studySessionRepository.SaveChangesAsync();

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // ResumeAsync — продолжает сессию после паузы
    public async Task<SessionResponse> ResumeAsync(int sessionId, int userId)
    {
        var session = await _studySessionRepository.GetByIdAsync(sessionId);

        if (session == null) throw new NotFoundException("Session not found");
        if (session.UserId != userId) throw new ForbiddenException("Access denied");

        session.Resume();
        await _studySessionRepository.SaveChangesAsync();

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // CompleteAsync — завершает сессию, устанавливает EndedAt
    public async Task<SessionResponse> CompleteAsync(int sessionId, int userId)
    {
        var session = await _studySessionRepository.GetByIdAsync(sessionId);

        if (session == null) throw new NotFoundException("Session not found");
        if (session.UserId != userId) throw new ForbiddenException("Access denied");

        session.Complete();
        await _studySessionRepository.SaveChangesAsync();

        // сессия завершена — сбрасываем кэш всех трёх лидербордов
        try
        {
            await _cache.RemoveAsync("leaderboard:daily");
            await _cache.RemoveAsync("leaderboard:weekly");
            await _cache.RemoveAsync("leaderboard:monthly");
        }
        catch { }

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // GetUserSessionsAsync — возвращает все сессии пользователя, от новых к старым
    public async Task<List<SessionResponse>> GetUserSessionsAsync(int userId)
    {
        var sessions = await _studySessionRepository.GetByUserIdAsync(userId);

        return sessions
            .Select(s => new SessionResponse(s.Id, s.SubjectId, s.StartedAt, s.EndedAt, s.SessionStatus.ToString()))
            .ToList();
    }
}
