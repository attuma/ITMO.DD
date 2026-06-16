namespace StudentTracker.Application.DTO;

public record AuthResponse(int UserId, string Username, string Email, string Token);

// просто форма данных