namespace Tests.Auto
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Core.Client;
    using Core.Model;
    using FluentAssertions;
    using Moq;
    using SignalR;
    using Tests.Common;
    using Xunit;
    using Zagidziran.Concurrent.AwaitableWaitHandle;

    public class FunctionalTests
    {
        private readonly Configuration configuration = Configuration.GetInstance();

        [Theory]
        [AutoData]
        public async Task ShouldSendAndReceiveMessage(string userId, string groupId, string text)
        {
            Func<Message, bool> messageFilter = message =>
            {
                return message.AuthorId == userId
                       && message.GroupId == groupId
                       && message.Text == text;
            };

            using var messageReceivedEvent = new AutoResetEvent(false);
            var clientMock = new Mock<INextChatClient>();


            clientMock
                .Setup(m => m.OnMessage(It.Is<Message>(m => messageFilter(m))))
                .Callback((Message _) => messageReceivedEvent.Set());

            var client = await NextChatServerClientFactory.CreateClient(
                configuration.NextChat with { UserId = userId },
                clientMock.Object);

            try
            {
                await client.JoinGroup(groupId);
                await client.SendMessage(groupId, text);

                await messageReceivedEvent.WithTimeout(configuration.FunctionalTests.MessageWaitTimeout);
            }
            finally
            {
                ((IDisposable)client).Dispose();
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldSendAndReceiveManyMessages(string userId, string groupId, [Count(100)] string[] texts)
        {
            Func<Message, bool> messageFilter = message =>
            {
                return message.AuthorId == userId
                       && message.GroupId == groupId;
            };

            var receivedMessages = new ConcurrentBag<Message>();

            using var messagesReceivedCountdown = new CountdownEvent(100);
            var clientMock = new Mock<INextChatClient>();


            clientMock
                .Setup(m => m.OnMessage(It.Is<Message>(m => messageFilter(m))))
                .Callback((Message msg) =>
                {
                    receivedMessages.Add(msg);
                    messagesReceivedCountdown.Signal();
                });

            var client = await NextChatServerClientFactory.CreateClient(
                configuration.NextChat with { UserId = userId },
                clientMock.Object);

            try
            {
                await client.JoinGroup(groupId);

                await Task.WhenAll(texts.Select(text => client.SendMessage(groupId, text)));

                await messagesReceivedCountdown.WaitHandle.WithTimeout(configuration.FunctionalTests.MessagesBulkWaitTimeout);
            }
            finally
            {
                ((IDisposable)client).Dispose();
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldReceiveOtherUsersMessage(
            string userId,
            string otherUserId,
            string groupId,
            string text)
        {
            Func<Message, bool> messageFilter = message =>
            {
                return message.AuthorId == otherUserId
                       && message.GroupId == groupId
                       && message.Text == text;
            };

            using var messageReceivedEvent = new AutoResetEvent(false);
            var clientMock = new Mock<INextChatClient>();


            clientMock
                .Setup(m => m.OnMessage(It.Is<Message>(m => messageFilter(m))))
                .Callback((Message _) => messageReceivedEvent.Set());

            var client = await NextChatServerClientFactory.CreateClient(
                configuration.NextChat with { UserId = userId },
                clientMock.Object);
            var otherUserClient = await NextChatServerClientFactory.CreateClient(
                configuration.NextChat with { UserId = otherUserId },
                Mock.Of<INextChatClient>());

            try
            {
                await client.JoinGroup(groupId);
                await otherUserClient.JoinGroup(groupId);

                await otherUserClient.SendMessage(groupId, text);

                await messageReceivedEvent.WithTimeout(configuration.FunctionalTests.MessageWaitTimeout);
            }
            finally
            {
                ((IDisposable)client).Dispose();
                ((IDisposable)otherUserClient).Dispose();
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldReturnNoRoom([Count(21)]string[] userIds, string groupId)
        {
             var createClientTasks = userIds.Select(userId => NextChatServerClientFactory.CreateClient(
                configuration.NextChat with { UserId = userId },
                Mock.Of<INextChatClient>()))
                 .ToList();

             await Task.WhenAll(createClientTasks);

             var clients = createClientTasks
                 .Select(t => t.Result)
                 .ToList();

            try
            {
                for (int i = 0; i < 20; i++)
                {
                    await clients[i].JoinGroup(groupId);
                }

                var lastUserJoinResult = await clients[20].JoinGroup(groupId);
                lastUserJoinResult.Should().Be(GroupJoinResult.NoRoom);
            }
            finally
            {
                foreach (var disposable in clients.Cast<IDisposable>())
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
