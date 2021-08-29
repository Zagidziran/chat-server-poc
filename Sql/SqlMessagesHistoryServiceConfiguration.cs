namespace Sql
{
    public record SqlMessagesHistoryServiceConfiguration
    {
        public string ConnectionString { get; init; } = default!;
    }
}
