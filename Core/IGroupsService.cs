namespace Core
{
    using Core.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGroupsService
    {
        Task<IReadOnlyCollection<Group>> ListGroups();

        Task<Group> CreateGroup(string groupId);

        Task<GroupJoinResult> Join(string groupId, string userId);

        Task Leave(string groupId, string userId);

        Task<IReadOnlyCollection<string>> GetParticipantIds(string groupId);
    }
}
