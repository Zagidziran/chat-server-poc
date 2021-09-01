namespace NextClient.CommandLine
{
    using global::CommandLine;

    [Verb("connect", HelpText = "Connect to Next Chat server")]
    public record ConnectCommand
    {
        [Value(0, Required = true, HelpText = "Next Chat server URI")]
        public string Uri { get; init; }

        [Value(1, Required = true, HelpText = "User name in chat")]
        public string UserId { get; init; }
    }
}
