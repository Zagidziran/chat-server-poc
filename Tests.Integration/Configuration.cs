namespace Tests.Integration
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Redis;
    using Sql;

    internal sealed record Configuration
    {
        private static readonly Lazy<Configuration> instance = new Lazy<Configuration>(Load);

        public RedisClientConfiguration Redis { get; init; } = new RedisClientConfiguration();

        public RedisGroupsServiceConfiguration Groups { get; init; } = new RedisGroupsServiceConfiguration();

        public SqlMessagesHistoryServiceConfiguration Sql { get; init; } = new SqlMessagesHistoryServiceConfiguration();

        public static Configuration GetInstance() => instance.Value;

        private static Configuration Load()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddYamlFile("configuration.yaml");
            return  configurationBuilder.Build().Get<Configuration>();
        }
    }
}
