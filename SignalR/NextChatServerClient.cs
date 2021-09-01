namespace SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Client;
    using Core.Model;
    using Core.Server;
    using Microsoft.AspNetCore.SignalR.Client;

    internal sealed class NextChatServerClient : INextChatServer, IDisposable
    {
        private readonly HubConnection connection;

        private readonly INextChatClient messageHandler;

        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public NextChatServerClient(HubConnection connection, INextChatClient messageHandler)
        {
            this.connection = connection;
            this.messageHandler = messageHandler;

            disposables.Add(
                this.connection.On<IReadOnlyCollection<Message>>(
                    nameof(this.messageHandler.OnHistory),
                    this.messageHandler.OnHistory));

            disposables.Add(
                this.connection.On<Message>(
                    nameof(this.messageHandler.OnMessage),
                    messageHandler.OnMessage));
        }

        public async Task CreateGroup(string groupId)
        {
            await this.connection.InvokeAsync(nameof(CreateGroup), groupId);
        }

        public async Task<GroupJoinResult> JoinGroup(string groupId)
        {
            return await this.connection.InvokeAsync<GroupJoinResult>(nameof(JoinGroup), groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            await this.connection.InvokeAsync(nameof(LeaveGroup), groupId);
        }

        public async Task<IReadOnlyCollection<Group>> ListGroups()
        {
            return await this.connection.InvokeAsync<IReadOnlyCollection<Group>>(nameof(ListGroups));
        }

        public async Task SendMessage(string groupId, string text)
        {
            await this.connection.InvokeAsync(nameof(SendMessage), groupId, text);
        }

        public void Dispose()
        {
            foreach (var disposable in this.disposables)
            {
                disposable.Dispose();
            }
            
            this.connection.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
