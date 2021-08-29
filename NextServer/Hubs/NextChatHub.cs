namespace NextServer.Hubs
{
    using Core;
    using Core.Client;
    using Core.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Authorize]
    public class NextChatHub : Hub<INextChatClient>, IMessagesHandler
    {
        private readonly IGroupsService groupsService;

        private readonly IMessagesTransportService messagesTransportService;

        private readonly IMessagesHistoryService messagesHistoryService;

        public NextChatHub(
            IGroupsService groupsService,
            IMessagesTransportService messagesTransportService,
            IMessagesHistoryService messagesHistoryService)
        {
            this.groupsService = groupsService;
            this.messagesTransportService = messagesTransportService;
            this.messagesHistoryService = messagesHistoryService;
            this.messagesTransportService.SetMessageHandler(this);
        }

        public async Task<GroupJoinResult> JoinGroup(string groupId)
        {
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupId);
            var result = await this.groupsService.Join(groupId, this.Context.UserIdentifier!);

            if (result == GroupJoinResult.Ok)
            {
                await this.messagesTransportService.Subscribe(groupId);
                var history = await this.messagesHistoryService.GetHistory(groupId);
                await this.Clients.Caller.OnHistory(history);
            }

            return result;
        }

        public async Task LeaveGroup(string groupId)
        {
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupId);
            await this.groupsService.Leave(groupId, this.Context.UserIdentifier!);
        }

        public async Task<IReadOnlyCollection<Group>> ListGroups()
        {
            return await this.groupsService.ListGroups();
        }

        public async Task CreateGroup(string groupId)
        {
            await this.groupsService.CreateGroup(groupId);
        }

        public async Task SendMessage(string groupId, string text)
        {
            var message = new Message
            {
                GroupId = groupId,
                Text = text,
                AuthorId = this.Context.UserIdentifier!,
            };

            var messageWithDate = await this.messagesHistoryService.CreateMessage(message);
            await this.messagesTransportService.SendMessage(messageWithDate);
        }

        async Task IMessagesHandler.OnMessage(Message message)
        {
            await this.Clients.Groups(message.GroupId).OnMessage(message);
        }
    }
}
