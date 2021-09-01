namespace NextClient.CommandLine
{
    using global::CommandLine;

    [Verb("create", HelpText = "Create a group")]
    public record CreateCommand
    {
        [Value(0, Required = true, HelpText = "Name of the group to create")]
        public string GroupId { get; init; }
    }
}
