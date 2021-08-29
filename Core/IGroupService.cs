namespace Core
{
    using Core.Model;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGroupService
    {
        Task<IReadOnlyCollection<Group>> ListGroups();

        Task<Group> CreateGroup(string dispalyName);
    }
}
