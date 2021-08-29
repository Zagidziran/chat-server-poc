namespace Tests.Integration
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Core;
    using Core.Model;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Redis;
    using Xunit;

    public class RedisGroupServiceTests : IDisposable
    {
        private readonly RedisGroupsService sut;

        private readonly ServiceProvider sp;

        public RedisGroupServiceTests()
        {
            var sc = new ServiceCollection();
            sc.AddRedisStorage(Configuration.GetInstance().Redis);
            this.sp = sc.BuildServiceProvider();
            this.sut = (RedisGroupsService)this.sp.GetRequiredService<IGroupsService>();
        }

        [Theory]
        [AutoData]
        public async Task ShouldCreateAndListGroup(string groupId)
        {
            Group group = null;
            try
            {
                group = await this.sut.CreateGroup(groupId);
                var currentGroups = await this.sut.ListGroups();

                group.GroupId.Should().Be(groupId);
                currentGroups.Should().Contain(group);
            }
            finally
            {
                await this.sut.DeleteGroup(group?.GroupId ?? groupId);
            }
        }

        [Fact]
        public async Task ShouldSortGroupsSomehow()
        {
            var groupsToCreate = new[]
            {
                "CGroup",
                "zGroup",
                "aGroup",
                "zgroup",
                "abGroup",
            };

            var expectedOrder = groupsToCreate.ToArray();
            Array.Sort(expectedOrder, StringComparer.Ordinal);

            Group[] groups = null;
            try
            {
                groups = await Task.WhenAll(groupsToCreate.Select(groupId => this.sut.CreateGroup(groupId)));
                var currentGroups = await this.sut.ListGroups();

                currentGroups.Select(g => g.GroupId)
                    .Should()
                    .BeEquivalentTo(
                        expectedOrder, 
                        o => o.WithStrictOrderingFor(_ => _));
            }
            finally
            {
                var groupsToDelete = groups?.Select(g => g.GroupId) ?? groupsToCreate;
                await Task.WhenAll(groupsToDelete.Select(groupId => this.sut.DeleteGroup(groupId)));
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldJoinAndListParticipant(string groupId, string userId)
        {
            Group group = null;
            try
            {
                group = await this.sut.CreateGroup(groupId);
                var joinResult = await this.sut.Join(userId, group.GroupId);

                var participants = await this.sut.GetParticipantIds(group.GroupId);

                joinResult.Should().Be(GroupJoinResult.Ok);
                participants.Should().BeEquivalentTo(userId);
            }
            finally
            {
                await this.sut.DeleteGroup(group?.GroupId ?? groupId);
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldLeaveGroup(string groupId, string userId)
        {
            Group group = null;
            try
            {
                group = await this.sut.CreateGroup(groupId);
                await this.sut.Join(userId, group.GroupId);
                await this.sut.Leave(userId, group.GroupId);
                
                var participants = await this.sut.GetParticipantIds(group.GroupId);

                participants.Should().BeEmpty();
            }
            finally
            {
                await this.sut.DeleteGroup(group?.GroupId ?? groupId);
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldNotAddTooManyParticipants(string groupId, [Count(60)]string[] userIds)
        {
            Group group = null;
            try
            {
                group = await this.sut.CreateGroup(groupId);
                foreach (var userId in userIds)
                {
                    await this.sut.Join(userId, group.GroupId);
                }

                var participants = await this.sut.GetParticipantIds(group.GroupId);

                participants.Should().BeEquivalentTo(userIds.Take(Configuration.GetInstance().Groups.MaxGroupMembers));
            }
            finally
            {
                await this.sut.DeleteGroup(group?.GroupId ?? groupId);
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldJoinWhenAfterOtherLeave(string groupId, [Count(20)] string[] userIds, string userIdOverLimit)
        {
            Group group = null;
            try
            {
                group = await this.sut.CreateGroup(groupId);
                foreach (var userId in userIds)
                {
                    await this.sut.Join(userId, group.GroupId);
                }

                await this.sut.Leave(userIds[new Random().Next(19)], group.GroupId);

                var result = await this.sut.Join(userIdOverLimit, group.GroupId);

                result.Should().Be(GroupJoinResult.Ok);
            }
            finally
            {
                await this.sut.DeleteGroup(group?.GroupId ?? groupId);
            }
        }

        [Theory]
        [AutoData]
        public async Task ShouldReturnNoRoom(string groupId, [Count(20)] string[] userIds, string userIdOverLimit)
        {
            Group group = null;
            try
            {
                group = await this.sut.CreateGroup(groupId);
                foreach (var userId in userIds)
                {
                    await this.sut.Join(userId, group.GroupId);
                }

                var result = await this.sut.Join(userIdOverLimit, group.GroupId);

                result.Should().Be(GroupJoinResult.NoRoom);
            }
            finally
            {
                await this.sut.DeleteGroup(group?.GroupId ?? groupId);
            }
        }

        public void Dispose()
        {
            this.sp.Dispose();
        }
    }
}
