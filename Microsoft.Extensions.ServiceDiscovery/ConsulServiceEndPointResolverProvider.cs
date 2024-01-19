using Consul;
using Microsoft.Extensions.ServiceDiscovery.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{
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