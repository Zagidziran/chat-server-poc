namespace Core.Client
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Model;

    public interface INextChatClient
    {
        Task OnMessage(Message message);

        Task OnHistory(IReadOnlyCollection<Message> history);
    }
}
