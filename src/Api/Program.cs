using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentTracker.Application.DTO;
using StudentTracker.Application.Exceptions;
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
builder.Services.AddScoped<IStudySessionRepository, StudySessionRepository>();
builder.Services.AddScoped<IStudySessionService, StudySessionService>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupMembershipRepository, GroupMembershipRepository>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

// Redis — кэш для лидерборда
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration["Redis"]);

var app = builder.Build();

// глобальная обработка ошибок — превращает исключения в правильные HTTP коды
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    var (status, message) = exception switch
    {
        NotFoundException e  => (404, e.Message),
        ForbiddenException e => (403, e.Message),
        BadRequestException e => (400, e.Message),
        ConflictException e  => (409, e.Message),
        _                    => (500, "Internal server error")
    };

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new { error = message });
}));

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

// POST /subjects — создать предмет в
app.MapPost("/subjects", async (SubjectRequest request, ISubjectService subjectService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await subjectService.CreateAsync(request, userId);
    return Results.Created($"/subjects/{result.SubjectId}", result);
}).RequireAuthorization();

// GET /subjects — получить все предметы пользователя
app.MapGet("/subjects", async (ISubjectService subjectService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await subjectService.GetUserSubjectsAsync(userId);
    return Results.Ok(result);
}).RequireAuthorization();

// DELETE /subjects/{id} — мягкое удаление предмета (IsArchived = true)
app.MapDelete("/subjects/{id}", async (int id, ISubjectService subjectService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    await subjectService.ArchiveAsync(id, userId);
    return Results.NoContent();
}).RequireAuthorization();

// POST /groups/{id}/subjects — создать предмет для группы
app.MapPost("/groups/{id}/subjects", async (int id, SubjectRequest request, ISubjectService subjectService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await subjectService.CreateForGroupAsync(request, id, userId);
    return Results.Created($"/groups/{id}/subjects/{result.SubjectId}", result);
}).RequireAuthorization();

// GET /groups/{id}/subjects — получить предметы группы
app.MapGet("/groups/{id}/subjects", async (int id, ISubjectService subjectService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await subjectService.GetGroupSubjectsAsync(id, userId);
    return Results.Ok(result);
}).RequireAuthorization();

// POST /sessions — начать учебную сессию
app.MapPost("/sessions", async (StartSessionRequest request, IStudySessionService sessionService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await sessionService.StartAsync(request, userId);
    return Results.Created($"/sessions/{result.Id}", result);
}).RequireAuthorization();

// POST /sessions/{id}/pause — поставить сессию на паузу 
app.MapPost("/sessions/{id}/pause", async (int id, IStudySessionService sessionService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await sessionService.PauseAsync(id, userId);
    return Results.Ok(result);
}).RequireAuthorization();

// POST /sessions/{id}/resume — продолжить сессию после паузы 
app.MapPost("/sessions/{id}/resume", async (int id, IStudySessionService sessionService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await sessionService.ResumeAsync(id, userId);
    return Results.Ok(result);
}).RequireAuthorization();

// POST /sessions/{id}/complete — завершить сессию 
app.MapPost("/sessions/{id}/complete", async (int id, IStudySessionService sessionService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await sessionService.CompleteAsync(id, userId);
    return Results.Ok(result);
}).RequireAuthorization();

// GET /sessions — получить все сессии пользователя 
app.MapGet("/sessions", async (IStudySessionService sessionService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await sessionService.GetUserSessionsAsync(userId);
    return Results.Ok(result);
}).RequireAuthorization();

// POST /groups — создать группу
app.MapPost("/groups", async (CreateGroupRequest request, IGroupService groupService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await groupService.CreateAsync(request, userId);
    return Results.Created($"/groups/{result.Id}", result);
}).RequireAuthorization();

// POST /groups/{id}/join — вступить в группу
app.MapPost("/groups/{id}/join", async (int id, IGroupService groupService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await groupService.JoinAsync(new JoinGroupRequest(id), userId);
    return Results.Ok(result);
}).RequireAuthorization();

// POST /groups/{id}/leave — выйти из группы
app.MapPost("/groups/{id}/leave", async (int id, IGroupService groupService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    await groupService.LeaveAsync(id, userId);
    return Results.NoContent();
}).RequireAuthorization();

// GET /groups — получить все группы пользователя
app.MapGet("/groups", async (IGroupService groupService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await groupService.GetUserGroupsAsync(userId);
    return Results.Ok(result);
}).RequireAuthorization();

// GET /groups/{id}/members — получить участников группы
app.MapGet("/groups/{id}/members", async (int id, IGroupService groupService, ClaimsPrincipal user) =>
{
    var result = await groupService.GetMembersAsync(id);
    return Results.Ok(result);
}).RequireAuthorization();

// GET /leaderboard/daily — топ пользователей за сегодня
app.MapGet("/leaderboard/daily", async (ILeaderboardService leaderboardService) =>
{
    var result = await leaderboardService.GetDailyLeaderboardAsync();
    return Results.Ok(result);
}).RequireAuthorization();

// GET /leaderboard/weekly — топ пользователей за последние 7 дней
app.MapGet("/leaderboard/weekly", async (ILeaderboardService leaderboardService) =>
{
    var result = await leaderboardService.GetWeeklyLeaderboardAsync();
    return Results.Ok(result);
}).RequireAuthorization();

// GET /leaderboard/monthly — топ пользователей за последние 30 дней
app.MapGet("/leaderboard/monthly", async (ILeaderboardService leaderboardService) =>
{
    var result = await leaderboardService.GetMonthlyLeaderboardAsync();
    return Results.Ok(result);
}).RequireAuthorization();

app.Run();
