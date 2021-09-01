namespace Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Core;
    using Core.Client;
    using Core.Model;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NextServer;
    using Redis;
    using SignalR;
    using Xunit;
    using Zagidziran.Concurrent.AwaitableWaitHandle;

    public class HubTests : IDisposable
    {
        private readonly WebApplicationFactory<Startup> webApplicationFactory;

        private readonly HttpMessageHandler httpMessageHandler;

        private readonly HubConnection connection;

        private readonly NextChatServerClient client;

        private readonly Mock<INextChatClient> messagesHandlerMock;

        private readonly RedisGroupsService groupsService;

        private readonly string userId;

        public HubTests()
        {
            userId = Guid.NewGuid().ToString();
            this.webApplicationFactory = new WebApplicationFactory<Startup>();
            this.httpMessageHandler = webApplicationFactory.Server.CreateHandler();
            this.connection = new HubConnectionBuilder()
                .WithUrl(
                    "http://localhost/next-chat",
                    o =>
                    {
                        o.HttpMessageHandlerFactory = _ => this.httpMessageHandler;
                        o.Headers.Add("Authorization", this.userId);
                    })
                .Build();

            this.connection.StartAsync().GetAwaiter().GetResult();
            this.messagesHandlerMock = new Mock<INextChatClient>();
            this.client = new NextChatServerClient(this.connection, this.messagesHandlerMock.Object);
            this.groupsService = (RedisGroupsService)this.webApplicationFactory.Services.GetRequiredService<IGroupsService>();
        }

        [Fact]
        public async Task ShouldReturnUnauthorized()
        {
            var newConnection = new HubConnectionBuilder()
                .WithUrl(
                    "http://localhost/next-chat",
                    o =>
                    {
                        o.HttpMessageHandlerFactory = _ => this.httpMessageHandler;
                    })
                .Build();

            Func<Task> action = () => newConnection.StartAsync();

            (await action.Should().ThrowAsync<HttpRequestException>())
                .Where(ex => ex.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Theory]
        [AutoData]
        public async Task ShouldCreateAndRetrieveGroup(string groupId)
        {
            var expected = new[] { new Group(groupId) };
            try
            {
                await this.client.CreateGroup(groupId);
                var result = await this.client.ListGroups();

                result.Should().BeEquivalentTo(expected);
            }
            finally
            {
                await this.groupsService.DeleteGroup(groupId);
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldSendHistoryAfterGroupJoin(string groupId, string message)
        {
            Func<IReadOnlyCollection<Message>, bool> isExpectedHistory = history =>
                history.Count == 1
                && history.First().Text == message
                && history.First().GroupId == groupId;

            var historyReceivedEvent = new AutoResetEvent(false);
            this.messagesHandlerMock
                .Setup(m => m.OnHistory(It.IsAny<IReadOnlyCollection<Message>>()))
                .Callback((IReadOnlyCollection<Message> _) => historyReceivedEvent.Set())
                .Returns(Task.CompletedTask);

            try
            {
                await this.client.CreateGroup(groupId);
                await this.client.SendMessage(groupId, message);
                await this.client.JoinGroup(groupId);

                await historyReceivedEvent.WithTimeout(TimeSpan.FromSeconds(1));

                this.messagesHandlerMock
                    .Verify(m => m.OnHistory(It.Is<IReadOnlyCollection<Message>>(h => isExpectedHistory(h))));
            }
            finally
            {
                await this.groupsService.DeleteGroup(groupId);
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldSendAndReceiveMessage(string groupId, string text)
        {
            Func<Message, bool> isExpectedMessage = msg =>
                msg.GroupId == groupId
                && msg.Text == text
                && msg.AuthorId == this.userId;

            var messageReceivedEvent = new AutoResetEvent(false);

            this.messagesHandlerMock
                .Setup(m => m.OnMessage(It.IsAny<Message>()))
                .Callback((Message _) => messageReceivedEvent.Set())
                .Returns(Task.CompletedTask);

            try
            {
                await this.client.CreateGroup(groupId);
                await this.client.JoinGroup(groupId);
                await this.client.SendMessage(groupId, text);

                await messageReceivedEvent.WithTimeout(TimeSpan.FromSeconds(1000)); ;

                this.messagesHandlerMock
                    .Verify(m => m.OnMessage(It.Is<Message>(message => isExpectedMessage(message))));
            }
            finally
            {
                await this.groupsService.DeleteGroup(groupId);
            }
        }

        public void Dispose()
        {
            this.connection.DisposeAsync().GetAwaiter().GetResult();
            this.httpMessageHandler.Dispose();
            this.webApplicationFactory.Dispose();
        }
    }
}
