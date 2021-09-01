namespace NextServer.Hubs
{
    using System;
    using Core;
    using Core.Client;
    using Core.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Server;
    using NextServer.Configuration;
    using Services;

    [Authorize]
    public class NextChatHub : Hub<INextChatClient>, INextChatServer
    {
        private readonly IGroupsService groupsService;

        private readonly IMessagesTransportService messagesTransportService;

        private readonly IMessagesHistoryService messagesHistoryService;

        private readonly IGroupStatusCache groupStatusCache;

        private readonly ServerConfiguration serverConfiguration;

        public NextChatHub(
            IGroupsService groupsService,
            IMessagesTransportService messagesTransportService,
            IMessagesHistoryService messagesHistoryService,
            IGroupStatusCache groupStatusCache,
            ServerConfiguration serverConfiguration)
        {
            this.groupsService = groupsService;
            this.messagesTransportService = messagesTransportService;
            this.messagesHistoryService = messagesHistoryService;
            this.groupStatusCache = groupStatusCache;
            this.serverConfiguration = serverConfiguration;
        }

        public async Task<GroupJoinResult> JoinGroup(string groupId)
        {
            var userId = this.Context.UserIdentifier!;
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupId);

            var result = await this.groupsService.Join(groupId, userId);

            if (result == GroupJoinResult.Ok)
            {
                if (JoinGroupVerdict.NeedSubscribe == this.groupStatusCache.OnJoinGroup(userId, groupId))
                {
                    await this.messagesTransportService.Subscribe(groupId);
                }

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

            var saveMessageTask = this.messagesHistoryService.CreateMessage(message);
            var timeoutTask = Task.Delay(this.serverConfiguration.MessagesHistory.SaveMessageTimeout);
            
            if (saveMessageTask == await Task.WhenAny(saveMessageTask, timeoutTask))
            {
                await this.messagesTransportService.SendMessage(saveMessageTask.Result);
            }
            else
            {
                // We still want to send message even history persistence is down
                await this.messagesTransportService.SendMessage(message with { SendTime = DateTimeOffset.UtcNow });
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = this.Context.UserIdentifier!;
            var groupsToLeave = this.groupStatusCache.OnDisconnect(userId);

            foreach (var groupId in groupsToLeave)
            {
                // TODO: Improve repository contract with batching for the case
                await this.groupsService.Leave(groupId, userId);
            }
        }
    }
}
