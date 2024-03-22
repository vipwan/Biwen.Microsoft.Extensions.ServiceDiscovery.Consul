namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{
    using System.Diagnostics.CodeAnalysis;
    internal class ConsulServiceEndPointResolverProvider(IConsulClient consulClient) : IServiceEndPointResolverProvider
    {
        private readonly IConsulClient _consulClient = consulClient;

        public bool TryCreateResolver(string serviceName, [NotNullWhen(true)] out IServiceEndPointProvider? resolver)
        {
            resolver = new ConsulServiceEndPointProvider(serviceName, _consulClient);
            return true;
        }
    }
}