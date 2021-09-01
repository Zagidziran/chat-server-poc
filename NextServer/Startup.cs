namespace NextServer
{
    using System.Text.Json;
    using Core;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NextServer.Authentication;
    using NextServer.Hubs;
    using Redis;
    using Services;
    using Sql;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = this.Configuration.Get<Configuration.Configuration>();

            services
                .AddSingleton(configuration.Server)
                .AddServices()
                .AddAuthentication()
                .AddScheme<SimpleAuthenticationHandlerOptions, SimpleAuthenticationHandler>(
                    AuthenticationSchemes.Simple,
                    null);

            services.AddAuthorization(o =>
            {
                o.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(AuthenticationSchemes.Simple)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services
                .AddRedisStorage(configuration.Redis)
                .AddSqlStorage(configuration.Sql)
                .AddSingleton<IMessagesHandler, MessagesHandler>()
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
