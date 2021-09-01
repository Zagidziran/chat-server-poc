namespace Core.Server
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Model;

    public interface INextChatServer
    {
        Task<GroupJoinResult> JoinGroup(string groupId);

        Task LeaveGroup(string groupId);

        Task<IReadOnlyCollection<Group>> ListGroups();

        Task CreateGroup(string groupId);

        Task SendMessage(string groupId, string text);
    }
}
