namespace NextServer
{
    using System.Text.Json;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NextServer.Authentication;
    using NextServer.Hubs;
    using Redis;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = this.Configuration.Get<Configuration>();

            services
                .AddAuthentication()
                .AddScheme<SimpleAuthenticationHandlerOptions, SimpleAuthenticationHandler>(
                    "simple",
                    null);

            services.AddAuthorization(o =>
            {
                o.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes("simple")
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services
                .AddRedisStorage(configuration.Redis)
                .AddSqlStorage(configuration.Sql)
                .AddSignalR()
                .AddJsonProtocol(o =>
                {
                    o.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<NextChatHub>("/next-chat");
                });
        }
    }
}
