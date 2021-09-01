namespace NextServer.Hubs
{
    using System.Threading.Tasks;
    using Core;
    using Core.Client;
    using Core.Model;
    using Microsoft.AspNetCore.SignalR;

    internal sealed class MessagesHandler : IMessagesHandler
    {
        private readonly IHubContext<NextChatHub, INextChatClient> hubContext;

        public MessagesHandler(IHubContext<NextChatHub, INextChatClient> hubContext)
        {
            this.hubContext = hubContext;
        }

        async Task IMessagesHandler.OnMessage(Message message)
        {
            await this.hubContext.Clients.Groups(message.GroupId).OnMessage(message);
        }
    }
}
