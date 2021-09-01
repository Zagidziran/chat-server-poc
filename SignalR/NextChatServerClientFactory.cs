namespace SignalR
{
    using System.Threading.Tasks;
    using Core;
    using Core.Client;
    using Core.Server;
    using Microsoft.AspNetCore.SignalR.Client;

    public static class NextChatServerClientFactory
    {
        public static async Task<INextChatServer> CreateClient(
            NextChatConfiguration configuration,
            INextChatClient handler)
        {
            var hubBuilder = new HubConnectionBuilder()
                .WithUrl(configuration.Uri,
                    opt => opt.Headers.Add("Authorization", configuration.UserId));

            var connection = hubBuilder.Build();
            await connection.StartAsync();
            
            return new NextChatServerClient(connection, handler);
        }
    }
}
