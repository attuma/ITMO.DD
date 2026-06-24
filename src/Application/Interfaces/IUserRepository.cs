using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
// Реализация будет находится в Infrastructure

// GetByEmailAsync и GetByUsernameAsync для ищет по почте или имени 
// ExistsByEmailAsync и ExistsByUsernameAsync проверяет занят ли почту или имя 
// AddAsync добавляет нового пользователя 
// SaveChangesAsync отправляет все изменения в БД