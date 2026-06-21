using StudentTracker.Application.DTO;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;


namespace StudentTracker.Application.Services;

// сервис для работы с учебными сессиями таймер
public class StudySessionService : IStudySessionService
{
    private readonly IStudySessionRepository _studySessionRepository;

    public StudySessionService(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    // StartAsync — создаёт новую сессию со статусом Active
    public async Task<SessionResponse> StartAsync(StartSessionRequest request, int userId)
    {
        var session = new StudySession(userId, request.SubjectId);

        await _studySessionRepository.AddAsync(session);
        await _studySessionRepository.SaveChangesAsync();

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // PauseAsync — ставит сессию на паузу, проверяет что сессия принадлежит пользователю
    public async Task<SessionResponse> PauseAsync(int sessionId, int userId)
    {
        var session = await _studySessionRepository.GetByIdAsync(sessionId);

        if (session == null) throw new Exception("Session not found");
        if (session.UserId != userId) throw new Exception("Access denied");

        session.Pause();
        await _studySessionRepository.SaveChangesAsync();

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // ResumeAsync — продолжает сессию после паузы
    public async Task<SessionResponse> ResumeAsync(int sessionId, int userId)
    {
        var session = await _studySessionRepository.GetByIdAsync(sessionId);

        if (session == null) throw new Exception("Session not found");
        if (session.UserId != userId) throw new Exception("Access denied");

        session.Resume();
        await _studySessionRepository.SaveChangesAsync();

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // CompleteAsync — завершает сессию, устанавливает EndedAt
    public async Task<SessionResponse> CompleteAsync(int sessionId, int userId)
    {
        var session = await _studySessionRepository.GetByIdAsync(sessionId);

        if (session == null) throw new Exception("Session not found");
        if (session.UserId != userId) throw new Exception("Access denied");

        session.Complete();
        await _studySessionRepository.SaveChangesAsync();

        return new SessionResponse(session.Id, session.SubjectId, session.StartedAt, session.EndedAt, session.SessionStatus.ToString());
    }

    // GetUserSessionsAsync — возвращает все сессии пользователя
    public async Task<List<SessionResponse>> GetUserSessionsAsync(int userId)
    {
        var sessions = await _studySessionRepository.GetByUserIdAsync(userId);

        return sessions
            .Select(s => new SessionResponse(s.Id, s.SubjectId, s.StartedAt, s.EndedAt, s.SessionStatus.ToString()))
            .ToList();
    }

}
