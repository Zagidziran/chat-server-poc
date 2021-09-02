namespace Tests.Auto
{
    using System;

    internal sealed record PerformanceTestsConfiguration
    {
        public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(60);

        public int Rate { get; init; } = 1000;

        public int MessageLength { get; init; } = 1000;

        public TimeSpan ExpectedMeanLatency { get; init; } = TimeSpan.FromMilliseconds(100);
    }
}