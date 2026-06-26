using StudentTracker.Application.DTO;
using StudentTracker.Application.Exceptions;
using StudentTracker.Application.Interfaces;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Application.Services;

// сервис для работы с группами
public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMembershipRepository _groupMembershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStudySessionRepository _sessionRepository;

    public GroupService(IGroupRepository groupRepository, IGroupMembershipRepository groupMembershipRepository, IUserRepository userRepository, IStudySessionRepository sessionRepository)
    {
        _groupRepository = groupRepository;
        _groupMembershipRepository = groupMembershipRepository;
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }

    // CreateAsync — создаёт группу и добавляет создателя как GroupOwner
    public async Task<GroupResponse> CreateAsync(CreateGroupRequest request, int userId)
    {
        var group = new Group(request.GroupName, request.Description, userId);
        await _groupRepository.AddAsync(group);
        await _groupRepository.SaveChangesAsync();

        var membership = new GroupMembership(group.Id, userId, GroupRole.GroupOwner);
        await _groupMembershipRepository.AddAsync(membership);
        await _groupMembershipRepository.SaveChangesAsync();

        return new GroupResponse(group.Id, group.GroupName, group.JoinCode, group.CreatedAt);
    }

    // JoinAsync — добавляет пользователя в группу по коду
    public async Task<GroupResponse> JoinAsync(JoinGroupRequest request, int userId)
    {
        var group = await _groupRepository.GetByJoinCodeAsync(request.JoinCode);
        if (group == null) throw new NotFoundException("Group not found");

        // ищем любую запись — активную или с LeftAt (вышел раньше)
        var existing = await _groupMembershipRepository.GetByUserAndGroupIncludingLeftAsync(userId, group.Id);

        if (existing != null && existing.LeftAt == null)
            throw new ConflictException("Already a member");

        if (existing != null && existing.LeftAt != null)
        {
            // пользователь уже был в группе — реактивируем запись вместо создания новой
            existing.Rejoin();
        }
        else
        {
            var membership = new GroupMembership(group.Id, userId, GroupRole.Member);
            await _groupMembershipRepository.AddAsync(membership);
        }

        await _groupMembershipRepository.SaveChangesAsync();

        return new GroupResponse(group.Id, group.GroupName, group.JoinCode, group.CreatedAt);
    }

    // ArchiveAsync — архивирует группу (is_archived = true), только владелец
    public async Task ArchiveAsync(int groupId, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null) throw new NotFoundException("Group not found");
        if (group.OwnerUserId != userId) throw new ForbiddenException("Only the group owner can archive the group");

        group.Archive();
        await _groupRepository.SaveChangesAsync();
    }

    // LeaveAsync — удаляет пользователя из группы через метод Domain
    public async Task LeaveAsync(int groupId, int userId)
    {
        var membership = await _groupMembershipRepository.GetByUserAndGroupAsync(userId, groupId);
        if (membership == null) throw new ForbiddenException("Not a member");

        membership.Leave();
        await _groupMembershipRepository.SaveChangesAsync();
    }

    // GetMembersAsync — возвращает участников с признаком "учится сейчас" и временем за сегодня
    public async Task<List<MemberResponse>> GetMembersAsync(int groupId)
    {
        var memberships = await _groupMembershipRepository.GetByGroupIdAsync(groupId);

        var result = new List<MemberResponse>();
        foreach (var m in memberships)
        {
            var user = await _userRepository.GetByIdAsync(m.UserId);
            if (user == null) continue;

            var activeSession = await _sessionRepository.GetActiveByUserIdAsync(m.UserId);
            var isStudying = activeSession?.SessionStatus == Domain.Enums.StudySessionStatus.Active;
            var todaySeconds = await _sessionRepository.GetTodaySecondsAsync(m.UserId);

            result.Add(new MemberResponse(m.UserId, user.UserName, m.GroupRole, isStudying, todaySeconds));
        }
        return result;
    }

    // GetUserGroupsAsync — возвращает все группы где пользователь является участником
    public async Task<List<GroupResponse>> GetUserGroupsAsync(int userId)
    {
        var groups = await _groupRepository.GetGroupsByMemberAsync(userId);
        return groups
            .Select(g => new GroupResponse(g.Id, g.GroupName, g.JoinCode, g.CreatedAt))
            .ToList();
    }
}
