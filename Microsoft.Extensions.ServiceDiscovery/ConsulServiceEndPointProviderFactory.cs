// Licensed to the Biwen.Microsoft.Extensions.ServiceDiscovery under one or more agreements.
// The Biwen.Microsoft.Extensions.ServiceDiscovery licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Biwen.Microsoft.Extensions.ServiceDiscovery;

internal class ConsulServiceEndPointProviderFactory(IConsulClient consulClient, ILogger<ConsulServiceEndPointProviderFactory> logger) : IServiceEndpointProviderFactory
{
    private readonly IConsulClient _consulClient = consulClient;
    private readonly ILogger<ConsulServiceEndPointProviderFactory> _logger = logger;

    public bool TryCreateProvider(ServiceEndpointQuery query, [NotNullWhen(true)] out IServiceEndpointProvider? resolver)
    {
        resolver = new ConsulServiceEndPointProvider(query, _consulClient, _logger);
        return true;
    }
}