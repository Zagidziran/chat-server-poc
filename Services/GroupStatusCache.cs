namespace Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class GroupStatusCache : IGroupStatusCache
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> userGroupsCache =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();

        private readonly ConcurrentDictionary<string, JoinGroupVerdict> joinedGroups =
            new ConcurrentDictionary<string, JoinGroupVerdict>();

        public ICollection<string> OnDisconnect(string userId)
        {
            if (this.userGroupsCache.TryRemove(userId, out var cache))
            {
                return cache.Keys;
            }

            return Array.Empty<string>();
        }

        public JoinGroupVerdict OnJoinGroup(string userId, string groupId)
        {
            this.userGroupsCache
                .GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>())
                .TryAdd(groupId, 0);

            return this.joinedGroups.AddOrUpdate(
                groupId,
                JoinGroupVerdict.NeedSubscribe,
                (_, _) => JoinGroupVerdict.SubscribeAlreadyRecommended);
        }
    }
}
