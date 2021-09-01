namespace Tests.Unit
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using FluentAssertions;
    using Services;
    using Tests.Common;
    using Xunit;

    public class GroupStatusCacheTests
    {
        private readonly GroupStatusCache sut = new GroupStatusCache();

        [Theory]
        [AutoData]
        public void ShouldRecommendJoinGroup(string userId, string groupId)
        {
            var result = this.sut.OnJoinGroup(userId, groupId);

            result.Should().Be(JoinGroupVerdict.NeedSubscribe);
        }

        [Theory]
        [AutoData]
        public void ShouldNotRecommendJoinGroupForSecondUser(string userId, string secondUserId, string groupId)
        {
            this.sut.OnJoinGroup(userId, groupId);
            var result = this.sut.OnJoinGroup(secondUserId, groupId);

            result.Should().Be(JoinGroupVerdict.SubscribeAlreadyRecommended);
        }

        [Theory]
        [AutoData]
        public void ShouldRecommendJoinForEachGroup(string userId, string[] groupIds)
        {
            var recommendations = groupIds
                .Select(groupId => this.sut.OnJoinGroup(userId, groupId))
                .ToList();

            recommendations.Should().AllBeEquivalentTo(JoinGroupVerdict.NeedSubscribe);
        }

        [Theory]
        [AutoData]
        public void ShouldRecommendJoinOnlyOnceForParallelScenario([Count(1000)] string[] userIds, string groupId)
        {
            var results = userIds
                .AsParallel()
                .Select(userId => this.sut.OnJoinGroup(userId, groupId))
                .ToList();

            results.Should().ContainSingle(verdict => verdict == JoinGroupVerdict.NeedSubscribe);
        }

        [Theory]
        [AutoData]
        public void ShouldReturnUserGroupsOnLeave(string userId, [Count(1000)] string[] groupIds)
        {
            foreach (var groupId in groupIds)
            {
                this.sut.OnJoinGroup(userId, groupId);
            }

            var groupsToLeave = this.sut.OnDisconnect(userId);

            groupsToLeave.Should().BeEquivalentTo(groupIds);
        }

        [Theory]
        [AutoData]
        public void ShouldReturnUserGroupsOnLeaveInParallelScenario(string userId, [Count(1000)] string[] groupIds)
        {
            Parallel.ForEach(groupIds, groupId => this.sut.OnJoinGroup(userId, groupId));

            var groupsToLeave = this.sut.OnDisconnect(userId);

            groupsToLeave.Should().BeEquivalentTo(groupIds);
        }

    }
}
