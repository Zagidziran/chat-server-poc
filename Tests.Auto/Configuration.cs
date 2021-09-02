namespace Tests.Auto
{
    using System;
    using Microsoft.Extensions.Configuration;
    using SignalR;

    internal sealed record Configuration
    {
        private static readonly Lazy<Configuration> instance = new Lazy<Configuration>(Load);

        public NextChatConfiguration NextChat { get; init; } = new NextChatConfiguration();

        public FunctionalTestsConfiguration FunctionalTests { get; init; } = new FunctionalTestsConfiguration();

        public PerformanceTestsConfiguration PerformanceTests { get; init; } = new PerformanceTestsConfiguration();
        
        public static Configuration GetInstance() => instance.Value;

        private static Configuration Load()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .AddYamlFile("configuration.yaml")
                .AddEnvironmentVariables();
            return configurationBuilder.Build().Get<Configuration>();
        }
    }
}
