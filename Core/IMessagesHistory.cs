namespace Core
{
    using Core.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMessagesHistory
    {
        Task CreateMessage(Message message);

        Task<IReadOnlyCollection<Message>> GetHistory(string gtoupId);
    }
}
