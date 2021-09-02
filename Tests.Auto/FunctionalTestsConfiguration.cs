namespace Tests.Auto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed record FunctionalTestsConfiguration
    {
        public TimeSpan MessageWaitTimeout { get; init; } = TimeSpan.FromSeconds(1);

        public TimeSpan MessagesBulkWaitTimeout { get; init; } = TimeSpan.FromSeconds(5);
    }
}
