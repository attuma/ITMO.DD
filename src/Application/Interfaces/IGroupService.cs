using StudentTracker.Application.DTO;

namespace StudentTracker.Application.Interfaces; 

public interface IGroupService
{
    Task<GroupResponse> CreateAsync(CreateGroupRequest createGroupRequest, int userId);
    Task<GroupResponse> JoinAsync(JoinGroupRequest joinGroupRequest, int userId);
    Task LeaveAsync(int groupId, int userId);
    Task<List<MemberResponse>> GetMembersAsync(int groupId);
    Task<List<GroupResponse>> GetUserGroupsAsync(int userId);
}