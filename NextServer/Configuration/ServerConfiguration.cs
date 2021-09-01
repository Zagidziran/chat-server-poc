namespace NextServer.Configuration
{
    public sealed class ServerConfiguration
    {
        public MessagesHistoryConfiguration MessagesHistory { get; init; } = new MessagesHistoryConfiguration();
    }
}
