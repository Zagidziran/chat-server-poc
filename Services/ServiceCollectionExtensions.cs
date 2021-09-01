namespace Services
{
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            return
                serviceCollection.AddSingleton<IGroupStatusCache, GroupStatusCache>();
        }
    }
}
