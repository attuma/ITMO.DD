using StudentTracker.Domain.Enums;
namespace StudentTracker.Application.DTO;

public record MemberResponse(int UserId, string Username, GroupRole GroupRole);