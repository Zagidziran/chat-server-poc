namespace Redis
{
    using Core;
    using Microsoft.Extensions.DependencyInjection;
    using Sql;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlStorage(
            this IServiceCollection serviceCollection,
            SqlMessagesHistoryServiceConfiguration configuration)
        {
            return serviceCollection
                .AddSingleton(configuration)
                .AddSingleton<IMessagesHistoryService, SqlMessagesHistoryService>();
        }
    }
}
