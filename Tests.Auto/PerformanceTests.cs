namespace Tests.Auto
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
    using Core.Client;
    using Core.Model;
    using FluentAssertions;
    using Moq;
    using NBomber.Contracts;
    using NBomber.CSharp;
    using NBomber.Plugins.Network.Ping;
    using Serilog;
    using SignalR;
    using Tests.Common;
    using Xunit;
    using Zagidziran.Concurrent.AwaitableWaitHandle;

    public class PerformanceTests
    {
        private readonly Configuration configuration = Configuration.GetInstance();

        [Theory]
        [AutoData]
        public async Task SendMessagePerformanceShouldBeOk(string userId, string groupId)
        {
            var client = await NextChatServerClientFactory.CreateClient(
                configuration.NextChat with { UserId = userId },
                Mock.Of<INextChatClient>());
            try
            {
                var random = new Random();
                var chars = Enumerable
                    .Range(0, this.configuration.PerformanceTests.MessageLength)
                    .Select(_ => (char) ('A' + random.Next(60)))
                    .ToArray();

                var message = new string(chars);

                await client.JoinGroup(groupId);

                var step = Step.Create("sendMessage", async context =>
                {
                    await client.SendMessage(groupId, message);
                    return Response.Ok();
                });

                var loadSimulation =
                    LoadSimulation.NewInjectPerSec(this.configuration.PerformanceTests.Rate,
                        this.configuration.PerformanceTests.Duration);

                var scenario = ScenarioBuilder
                    .CreateScenario("next-chat-messages-load", step)
                    .WithLoadSimulations(loadSimulation)
                    .WithoutWarmUp();

                var stat = NBomberRunner
                    .RegisterScenarios(scenario)
                    .Run();

                stat.FailCount.Should().Be(0);
                stat.OkCount.Should().BeGreaterThan(0);
                stat.ScenarioStats.First().StepStats.First().Ok.Latency.MeanMs.Should()
                    .BeLessThan(this.configuration.PerformanceTests.ExpectedMeanLatency.TotalMilliseconds);
            }
            finally
            {
                ((IDisposable)client).Dispose();
            }
        }
    }
}
