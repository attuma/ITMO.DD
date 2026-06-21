using StudentTracker.Application.DTO;

namespace StudentTracker.Application.Interfaces;

public interface IStudySessionService
{
    Task<SessionResponse> StartAsync(StartSessionRequest request, int userId);
    Task<SessionResponse> PauseAsync(int sessionId, int userId);
    Task<SessionResponse> ResumeAsync(int sessionId, int userId);
    Task<SessionResponse> CompleteAsync(int sessionId, int userId);
    Task<List<SessionResponse>> GetUserSessionsAsync(int userId);
}