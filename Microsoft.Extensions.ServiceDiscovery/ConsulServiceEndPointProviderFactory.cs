namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{
    using global::Microsoft.Extensions.ServiceDiscovery;
    using System.Diagnostics.CodeAnalysis;
    internal class ConsulServiceEndPointProviderFactory(IConsulClient consulClient) : IServiceEndPointProviderFactory
    {
        private readonly IConsulClient _consulClient = consulClient;

        public bool TryCreateProvider(ServiceEndPointQuery query, [NotNullWhen(true)] out IServiceEndPointProvider? resolver)
        {
            resolver = new ConsulServiceEndPointProvider(query, _consulClient);
            return true;
        }
    }
}