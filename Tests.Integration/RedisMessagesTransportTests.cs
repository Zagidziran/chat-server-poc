namespace Tests.Integration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Core;
    using Core.Model;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Redis;
    using Xunit;
    using Zagidziran.Concurrent.AwaitableWaitHandle;

    public class RedisMessagesTransportTests : IDisposable
    {
        private readonly RedisMessagesTransportService sut;

        private readonly RedisGroupsService groupService;

        private readonly Group testGroup;

        private readonly ServiceProvider sp;

        private readonly Mock<IMessagesHandler> messagesHandlerMock = new Mock<IMessagesHandler>();

        public RedisMessagesTransportTests()
        {
            var sc = new ServiceCollection();
            sc.AddRedisStorage(Configuration.GetInstance().Redis);
            sc.AddSingleton(this.messagesHandlerMock.Object);
            this.sp = sc.BuildServiceProvider();
            this.sut = (RedisMessagesTransportService)this.sp.GetRequiredService<IMessagesTransportService>();
            this.groupService = (RedisGroupsService)this.sp.GetRequiredService<IGroupsService>();
            this.testGroup = this.groupService.CreateGroup(Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            this.sut.Subscribe(testGroup.GroupId).GetAwaiter().GetResult();
        }

        [Theory]
        [AutoData]
        public async Task ShouldSendAndReceiveMessage(Message message)
        {
            var messageReceivedEvent = new AutoResetEvent(false);
            var effectiveMessage = message with { GroupId = this.testGroup.GroupId };
            this.messagesHandlerMock
                .Setup(h => h.OnMessage(effectiveMessage))
                .Callback((Message _) => messageReceivedEvent.Set());
            
            
            await this.sut.SendMessage(effectiveMessage);

            await messageReceivedEvent.WithTimeout(TimeSpan.FromSeconds(3));
        }

        [Theory]
        [AutoData]
        public async Task ShouldNotReceiveMessageFromRandomGroup(Message message)
        {
            Group group = null;
            try
            {
                var messageReceivedEvent = new AutoResetEvent(false);
                this.messagesHandlerMock
                    .Setup(h => h.OnMessage(message))
                    .Callback((Message _) => messageReceivedEvent.Set());

                group = await this.groupService.CreateGroup(message.GroupId);

                await this.sut.SendMessage(message);

                // A second is pretty enough for transport layer 
                Func<Task> waitMessageAction = async () => await messageReceivedEvent.WithTimeout(TimeSpan.FromSeconds(1));

                await waitMessageAction.Should().ThrowAsync<TimeoutException>();
            }
            finally
            {
                await this.groupService.DeleteGroup(group?.GroupId ?? message.GroupId);
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldNotReceiveMessageAfterUnsubscribe(Message message)
        {
            var messageReceivedEvent = new AutoResetEvent(false);
            var effectiveMessage = message with { GroupId = this.testGroup.GroupId };
            this.messagesHandlerMock
                .Setup(h => h.OnMessage(message))
                .Callback((Message _) => messageReceivedEvent.Set());

            await this.sut.Unsubscribe(this.testGroup.GroupId);
            await this.sut.SendMessage(effectiveMessage);

            // A second is pretty enough for transport layer 
            Func<Task> waitMessageAction = async () => await messageReceivedEvent.WithTimeout(TimeSpan.FromSeconds(1));

            await waitMessageAction.Should().ThrowAsync<TimeoutException>();
        }

        [Theory]
        [AutoData]
        public async Task ShouldReceiveMessageFromAnotherInstance(Message message)
        {
            var messageReceivedEvent = new AutoResetEvent(false);
            var effectiveMessage = message with { GroupId = this.testGroup.GroupId };
            this.messagesHandlerMock
                .Setup(h => h.OnMessage(effectiveMessage))
                .Callback((Message _) => messageReceivedEvent.Set());

            var anotherInstance = new RedisMessagesTransportService(
                Configuration.GetInstance().Redis,
                Mock.Of<IMessagesHandler>());

            await anotherInstance.SendMessage(effectiveMessage);

            await messageReceivedEvent.WithTimeout(TimeSpan.FromSeconds(10000));
        }

        public void Dispose()
        {
            this.groupService.DeleteGroup(this.testGroup.GroupId).Wait();
            this.sp.Dispose();
        }
    }
}
