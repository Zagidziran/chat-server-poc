namespace Tests.Integration
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Core;
    using Core.Model;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Redis;
    using Sql;
    using Xunit;

    public class SqlMessagesHistoryServiceTests : IDisposable
    {
        private readonly SqlMessagesHistoryService sut;

        private readonly ServiceProvider sp;

        public SqlMessagesHistoryServiceTests()
        {
            var sc = new ServiceCollection();
            sc.AddSqlStorage(Configuration.GetInstance().Sql);
            this.sp = sc.BuildServiceProvider();
            this.sut = (SqlMessagesHistoryService)this.sp.GetRequiredService<IMessagesHistoryService>();
        }

        [Theory]
        [AutoData]
        public async Task ShouldCreateAndRetrieveMessage(Message message)
        {
            var resultMessage = await this.sut.CreateMessage(message);

            try
            {
                var history = await sut.GetHistory(message.GroupId);

                history.Should().BeEquivalentTo(
                    new[] {message},
                    opt => opt.Excluding(m => m.SendTime));
            }
            finally
            {
                await this.sut.DeleteMessage(resultMessage);
            }
        }

        public void Dispose()
        {
            sp.Dispose();
        }
    }
}
