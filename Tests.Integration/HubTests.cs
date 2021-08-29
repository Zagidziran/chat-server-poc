namespace Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Core.Model;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.SignalR.Client;
    using NextServer;
    using Xunit;

    public class HubTests
    {
        public HubTests()
        {
        }

        [Theory]
        [AutoData]
        public async Task ShouldReturnUnauthorized(string userId)
        {
            using var factory = new WebApplicationFactory<Startup>();
            using var handler = factory.Server.CreateHandler();
            var connection = new HubConnectionBuilder()
                .WithUrl(
                    "http://localhost/next-chat",
                    o =>
                    {
                        o.HttpMessageHandlerFactory = _ => handler;
                        o.AccessTokenProvider = () => Task.FromResult(userId);
                    })
                .Build();

            await connection.StartAsync();
            
            Func<Task<IReadOnlyCollection<Group>>> action = async () => await connection.InvokeAsync<IReadOnlyCollection<Group>>("ListGroups");

            var ee = await connection.InvokeAsync<IReadOnlyCollection<Group>>("ListGroups");

            await action.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
