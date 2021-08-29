namespace Redis
{
    public record RedisClientConfiguration
    {
        public string ConnectionString { get; init; } = default!;
    }
}
