namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{
    using System.Diagnostics.CodeAnalysis;
    public class ConsulServiceEndPointResolverProvider(IConsulClient consulClient) : IServiceEndPointResolverProvider
    {
        private readonly IConsulClient _consulClient = consulClient;
        public bool TryCreateResolver(string serviceName, [NotNullWhen(true)] out IServiceEndPointResolver? resolver)
        {
            resolver = new ConsulServiceEndPointResolver(serviceName, _consulClient);
            return true;
        }
    }
}