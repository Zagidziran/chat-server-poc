namespace NextServer.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed record MessagesHistoryConfiguration
    {
        public TimeSpan SaveMessageTimeout { get; init; } = TimeSpan.FromMilliseconds(500);
    }
}
