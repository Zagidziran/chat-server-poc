namespace Sql
{
    using Core;
    using Microsoft.Extensions.DependencyInjection;

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
