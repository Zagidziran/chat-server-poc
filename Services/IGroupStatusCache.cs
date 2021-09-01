namespace Services
{
    using System;
    using System.Collections.Generic;

    public interface IGroupStatusCache
    {
        JoinGroupVerdict OnJoinGroup(string userId, string groupId);

        ICollection<string> OnDisconnect(string userId);
    }
}
