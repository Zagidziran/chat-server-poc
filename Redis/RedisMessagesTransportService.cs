namespace Redis
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Core;
    using Core.Model;
    using StackExchange.Redis;

    internal sealed class RedisMessagesTransportService : RedisServiceBase, IMessagesTransportService
    {
        private IMessagesHandler? handler;

        private readonly Lazy<Task<ISubscriber>> subscriber;

        public RedisMessagesTransportService(RedisClientConfiguration configuration)
            : base(configuration)
        {
            this.subscriber = new Lazy<Task<ISubscriber>>(this.CreateSubscriber);
        }

        public async Task SendMessage(Message message)
        {
            var multiplexer = await this.GetMultiplexer();

            var serializedMessage = JsonSerializer.Serialize(new
            {
                message.Text,
                message.AuthorId,
                message.SendTime,
            });

            await multiplexer.GetSubscriber().PublishAsync(message.GroupId, serializedMessage);
        }

        public void SetMessageHandler(IMessagesHandler handler)
        {
            this.handler = handler;
        }

        public async Task Subscribe(string groupId)
        {
            var sub = await this.GetSubscriber();
            await sub.SubscribeAsync(groupId, this.OnMessage);
        }

        public async Task Unsubscribe(string groupId)
        {
            var sub = await this.GetSubscriber();
            await sub.UnsubscribeAsync(groupId);
        }

        private async Task<ISubscriber> GetSubscriber()
        {
            return await this.subscriber.Value;
        }

        private async Task<ISubscriber> CreateSubscriber()
        {
            var multiplexer = await this.GetMultiplexer();
            return multiplexer.GetSubscriber();
        }

        private void OnMessage(RedisChannel channel, RedisValue message)
        {
            async Task ProcessMessage()
            {
                try
                {
                    var deserializedMessage = JsonSerializer.Deserialize<Message>(message.ToString());
                    var resultMessage = deserializedMessage with {GroupId = channel.ToString()};
                    await (this.handler?.OnMessage(resultMessage) ?? Task.CompletedTask);
                }
                catch
                {
                    // Log
                }
            }

            Task.Factory.StartNew(ProcessMessage);
        }
    }
}
