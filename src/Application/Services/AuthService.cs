using StudentTracker.Application.DTO;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Application.Services;
// просто сервис реги и входа тут все вместе смотри другие файлы 
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new Exception("Email already in use.");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username))
        {
            throw new Exception("Username already in use.");
        }

        var user = new User
            (
            request.Username,
            request.Email,
            HashPassword(request.Password),
            SystemRole.Student
            );


        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse(user.Id, user.UserName, user.Email, token);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {

        User? user;

        if (request.Login.Contains("@"))
        {
            user = await _userRepository.GetByEmailAsync(request.Login);
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(request.Login);
        }

        if (user == null)
            throw new Exception("User not found");

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new Exception("uncorrected password");
        }

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse(user.Id, user.UserName, user.Email, token);


    }


}