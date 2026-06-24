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

    public GroupService(IGroupRepository groupRepository, IGroupMembershipRepository groupMembershipRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _groupMembershipRepository = groupMembershipRepository;
        _userRepository = userRepository;
    }

    // CreateAsync — создаёт группу и добавляет создателя как GroupOwner
    public async Task<GroupResponse> CreateAsync(CreateGroupRequest request, int userId)
    {
        var group = new Group(request.GroupName, request.Description, userId);
        await _groupRepository.AddAsync(group);
        await _groupRepository.SaveChangesAsync();

        // создатель автоматически становится владельцем группы
        var membership = new GroupMembership(group.Id, userId, GroupRole.GroupOwner);
        await _groupMembershipRepository.AddAsync(membership);
        await _groupMembershipRepository.SaveChangesAsync();

        return new GroupResponse(group.Id, group.GroupName, group.CreatedAt);
    }

    // JoinAsync — добавляет пользователя в группу как Member
    public async Task<GroupResponse> JoinAsync(JoinGroupRequest request, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId);
        if (group == null) throw new NotFoundException("Group not found");

        // ищем любую запись — активную или с LeftAt (вышел раньше)
        var existing = await _groupMembershipRepository.GetByUserAndGroupIncludingLeftAsync(userId, request.GroupId);

        if (existing != null && existing.LeftAt == null)
            throw new ConflictException("Already a member");

        if (existing != null && existing.LeftAt != null)
        {
            // пользователь уже был в группе — реактивируем запись вместо создания новой
            existing.Rejoin();
        }
        else
        {
            var membership = new GroupMembership(request.GroupId, userId, GroupRole.Member);
            await _groupMembershipRepository.AddAsync(membership);
        }

        await _groupMembershipRepository.SaveChangesAsync();

        return new GroupResponse(group.Id, group.GroupName, group.CreatedAt);
    }

    // LeaveAsync — удаляет пользователя из группы через метод Domain
    public async Task LeaveAsync(int groupId, int userId)
    {
        var membership = await _groupMembershipRepository.GetByUserAndGroupAsync(userId, groupId);
        if (membership == null) throw new ForbiddenException("Not a member");

        membership.Leave();
        await _groupMembershipRepository.SaveChangesAsync();
    }

    // GetMembersAsync — возвращает список участников группы с именами
    public async Task<List<MemberResponse>> GetMembersAsync(int groupId)
    {
        var memberships = await _groupMembershipRepository.GetByGroupIdAsync(groupId);

        var result = new List<MemberResponse>();
        foreach (var m in memberships)
        {
            var user = await _userRepository.GetByIdAsync(m.UserId);
            if (user != null)
                result.Add(new MemberResponse(m.UserId, user.UserName, m.GroupRole));
        }
        return result;
    }

    // GetUserGroupsAsync — возвращает все группы где пользователь является участником
    public async Task<List<GroupResponse>> GetUserGroupsAsync(int userId)
    {
        var groups = await _groupRepository.GetGroupsByMemberAsync(userId);
        return groups
            .Select(g => new GroupResponse(g.Id, g.GroupName, g.CreatedAt))
            .ToList();
    }
}
