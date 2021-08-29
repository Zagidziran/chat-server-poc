namespace Core.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Core.Model;

    public interface INextChatClient
    {
        Task OnMessage(Message message);

        Task OnHistory(IReadOnlyCollection<Message> history);
    }
}
