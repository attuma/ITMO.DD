using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using StudentTracker.Infrastructure.Persistence;


namespace StudentTracker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }
    // -- GetByEmailAsync: SELECT* FROM users WHERE email = 'ivan@mail.com' LIMIT 1
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    // GetByUsernameAsync: SELECT* FROM users WHERE username = 'ivan' LIMIT 1
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
    }
    // ExistsByEmailAsync: SELECT EXISTS(SELECT 1 FROM users WHERE email = 'ivan@mail.com')
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }
    
    // ExistsByUsernameAsync: SELECT EXISTS(SELECT 1 FROM users WHERE username = 'ivan')
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _db.Users.AnyAsync(u => u.UserName == username);
    }

    // AddAsync: запоминает пользователя в EF Core, SQL ещё не выполняется
    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
    }

    // SaveChangesAsync: отправляет всё в БД одной транзакцией 

    // BEGIN;
    // INSERT INTO users(username, email, password_hash, system_role, created_at)
    // VALUES('ivan', 'ivan@mail.com', 'хэш пароль', 'student', '2026-06-17 12:00:00');
    // COMMIT;
    // если что-то не так 
    // ROLLBACK;

    // GetByIdAsync: SELECT * FROM users WHERE id = userId LIMIT 1
    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }




}