namespace Redis
{
    using Core;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisStorage(
            this IServiceCollection serviceCollection,
            RedisClientConfiguration clientConfiguration,
            RedisGroupsServiceConfiguration? groupsConfiguration = null)
        {
            return serviceCollection
                .AddSingleton(clientConfiguration)
                .AddSingleton(groupsConfiguration ?? new RedisGroupsServiceConfiguration())
                .AddSingleton<IGroupsService, RedisGroupsService>()
                .AddSingleton<IMessagesTransportService, RedisMessagesTransportService>();
        }
    }
}
