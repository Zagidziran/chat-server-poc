namespace NextClient.CommandLine
{
    using global::CommandLine;

    [Verb("say", HelpText = "Send a message to a group")]
    public record SayCommand
    {
        [Value(0, Required = true, HelpText = "Group name to send a message")]
        public string GroupId { get; init; }

        [Value(1, Required = true, HelpText = "A message to send")]
        public string Message { get; init; }
    }
}
