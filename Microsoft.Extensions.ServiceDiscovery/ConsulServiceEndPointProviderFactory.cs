namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{
    using global::Microsoft.Extensions.Logging;
    using global::Microsoft.Extensions.ServiceDiscovery;
    using System.Diagnostics.CodeAnalysis;

    internal class ConsulServiceEndPointProviderFactory(IConsulClient consulClient, ILogger<ConsulServiceEndPointProviderFactory> logger) : IServiceEndPointProviderFactory
    {
        private readonly IConsulClient _consulClient = consulClient;
        private readonly ILogger<ConsulServiceEndPointProviderFactory> _logger = logger;

        public bool TryCreateProvider(ServiceEndPointQuery query, [NotNullWhen(true)] out IServiceEndPointProvider? resolver)
        {
            resolver = new ConsulServiceEndPointProvider(query, _consulClient, _logger);
            return true;
        }
    }
}