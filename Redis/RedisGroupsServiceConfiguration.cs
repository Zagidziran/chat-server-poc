namespace Redis
{
    public record RedisGroupsServiceConfiguration
    {
        public string GroupsSetName { get; init; } = "chat-groups:set";

        public string GroupMemberSetPrefix { get; init; } = "chat-groups:members:";

        public ushort MaxGroupMembers { get; init; } = 20;
    }
}
