using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
