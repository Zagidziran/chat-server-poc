namespace Configurator
{
    using System;

    internal sealed record Configuration
    {
        public SqlConfiguration Sql { get; init; }

        public byte Attempts { get; init; } = 5;

        public TimeSpan AttemptDelay { get; init; } = TimeSpan.FromSeconds(5);
    }

    internal sealed record SqlConfiguration
    {
        public string ConnectionString { get; init; }
    }
}
