namespace Redis
{
    using System;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    internal class RedisServiceBase
    {
        private readonly Lazy<Task<IConnectionMultiplexer>> connectionMultiplexer;

        public RedisServiceBase(RedisClientConfiguration configuration)
        {
            this.Configuration = configuration;
            this.connectionMultiplexer = new Lazy<Task<IConnectionMultiplexer>>(this.CreateMultiplexer);
        }

        protected RedisClientConfiguration Configuration { get;  }

        protected async Task<IConnectionMultiplexer> GetMultiplexer()
        {
            return await this.connectionMultiplexer.Value;
        }

        private async Task<IConnectionMultiplexer> CreateMultiplexer()
        {
            return await ConnectionMultiplexer.ConnectAsync(ConfigurationOptions.Parse(this.Configuration.ConnectionString));
        }
    }
}
