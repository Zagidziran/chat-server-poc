namespace NextClient.CommandLine
{
    using global::CommandLine;

    [Verb("leave", HelpText = "Connect to Next Chat server.")]
    public record LeaveCommand
    {
        [Value(0, Required = true, HelpText = "Group name to leave")]
        public string GroupId { get; init; }
    }
}
