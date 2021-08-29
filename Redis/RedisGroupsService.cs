namespace Redis
{
    using System;
    using Core;
    using Core.Model;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    internal sealed class RedisGroupsService : RedisServiceBase, IGroupsService
    {
        private readonly RedisGroupsServiceConfiguration groupsConfiguration;

        public RedisGroupsService(
            RedisClientConfiguration clientConfiguration,
            RedisGroupsServiceConfiguration groupsConfiguration)
            : base(clientConfiguration)
        {
            this.groupsConfiguration = groupsConfiguration;
        }

        public async Task<Group> CreateGroup(string groupId)
        {
            var multiplexer = await this.GetMultiplexer();

            await multiplexer.GetDatabase()
                .SortedSetAddAsync(this.groupsConfiguration.GroupsSetName, groupId, groupId.CalculateScore());

            return new Group(groupId);
        }

        public async Task<IReadOnlyCollection<string>> GetParticipantIds(string groupId)
        {
            var multiplexer = await this.GetMultiplexer();

            var participantIds = await multiplexer.GetDatabase()
                .SortedSetRangeByRankAsync(
                    this.BuildKeyForGroupParticipantSet(groupId),
                    0,
                    this.groupsConfiguration.MaxGroupMembers - 1);

            return participantIds.ToStringArray();
        }

        public async Task Leave(string userId, string groupId)
        {
            var multiplexer = await this.GetMultiplexer();
            var database = multiplexer.GetDatabase();

            var groupKey = this.BuildKeyForGroupParticipantSet(groupId);

            await database
                .SortedSetRemoveAsync(groupKey, userId);
        }

        public async Task<GroupJoinResult> Join(string userId, string groupId)
        {
            var multiplexer = await this.GetMultiplexer();
            var database = multiplexer.GetDatabase();

            var score = this.CalculateScoreForNewGroupParticipationRecord();
            var groupKey = this.BuildKeyForGroupParticipantSet(groupId);

            await database
                .SortedSetAddAsync(
                    groupKey,
                    userId,
                    score,
                    When.NotExists);

            await database.SortedSetRemoveRangeByRankAsync(
                groupKey,
                this.groupsConfiguration.MaxGroupMembers,
                -1);

            var participantIds = await multiplexer.GetDatabase()
                .SortedSetRangeByRankAsync(this.BuildKeyForGroupParticipantSet(groupId), 0, groupsConfiguration.MaxGroupMembers - 1);

            return participantIds.Any(p => p.ToString().Equals(userId))
                ? GroupJoinResult.Ok
                : GroupJoinResult.NoRoom;
        }

        public async Task<IReadOnlyCollection<Group>> ListGroups()
        {
            var multiplexer = await this.GetMultiplexer();

            // No paging.
            var groupIds = await multiplexer.GetDatabase()
                .SortedSetRangeByScoreAsync(this.groupsConfiguration.GroupsSetName);

            return groupIds.Select(id => new Group(id.ToString())).ToList();
        }

        internal async Task DeleteGroup(string groupId)
        {
            var multiplexer = await this.GetMultiplexer();

            var database = multiplexer.GetDatabase();
            await database.SortedSetRemoveAsync(this.groupsConfiguration.GroupsSetName, groupId);

            await database.SortedSetRemoveRangeByRankAsync(this.BuildKeyForGroupParticipantSet(groupId), 0, -1);
        }

        private double CalculateScoreForNewGroupParticipationRecord()
        {
            return DateTimeOffset.UtcNow.Ticks;
        }

        private string BuildKeyForGroupParticipantSet(string groupId)
        {
            return this.groupsConfiguration.GroupMemberSetPrefix + groupId;
        }
    }
}
