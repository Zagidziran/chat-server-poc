namespace SignalR
{
    public sealed record NextChatConfiguration
    {
        public string Uri { get; init; }

        public string UserId { get; set; }
    }
}
