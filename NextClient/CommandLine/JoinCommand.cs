namespace NextClient.CommandLine
{
    using global::CommandLine;

    [Verb("join", HelpText = "Join a group")]
    public record JoinCommand
    {
        [Value(0, Required = true, HelpText = "Group name to join")]
        public string GroupId { get; init; }
    }
}
