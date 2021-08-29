namespace Core
{
    using Core.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMessagesHistoryService
    {
        Task<Message> CreateMessage(Message message);

        Task<IReadOnlyCollection<Message>> GetHistory(string groupId);
    }
}
