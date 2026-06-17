using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentTracker.Application.DTO;
using StudentTracker.Application.Interfaces;
using StudentTracker.Application.Services;
using StudentTracker.Infrastructure.Persistence;
using StudentTracker.Infrastructure.Repositories;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// подключение к PostgreSQL через строку из appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger — интерфейс для тестирования API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // добавляем кнопку Authorize в Swagger для JWT токена
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введи JWT токен"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// настройка JWT: проверяем подпись токена по секретному ключу
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:SecretKey"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

// регистрация сервисов в DI — говорим какой интерфейс = какой класс
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

var app = builder.Build();

// Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// включаем проверку JWT токена и авторизацию
app.UseAuthentication();
app.UseAuthorization();

// эндпоинт регистрации — POST /register
app.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    return Results.Created($"/users/{result.UserId}", result);
});

// эндпоинт входа — POST /login
app.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return Results.Ok(result);
});

// POST /subjects — создать предмет (нужен JWT токен)
app.MapPost("/subjects", async (SubjectRequest request, ISubjectService subjectService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await subjectService.CreateAsync(request, userId);
    return Results.Created($"/subjects/{result.SubjectId}", result);
}).RequireAuthorization();

// GET /subjects — получить все предметы пользователя (нужен JWT токен)
app.MapGet("/subjects", async (ISubjectService subjectService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await subjectService.GetUserSubjectsAsync(userId);
    return Results.Ok(result);
}).RequireAuthorization();

app.Run();
