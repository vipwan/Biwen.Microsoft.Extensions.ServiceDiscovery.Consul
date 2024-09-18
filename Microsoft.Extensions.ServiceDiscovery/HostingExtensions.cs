// Licensed to the Biwen.Microsoft.Extensions.ServiceDiscovery under one or more agreements.
// The Biwen.Microsoft.Extensions.ServiceDiscovery licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Exs = Consul.AspNetCore.ServiceCollectionExtensions;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    /// <summary>
    /// AddConsulServiceEndpointProvider, 
    /// before use this provider, you should call <see cref="Exs.AddConsul"/> to register Consul first
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddConsulServiceEndpointProvider(this IServiceCollection services)
    {
        services.AddServiceDiscoveryCore();
        services.AddSingleton<IServiceEndpointProviderFactory, ConsulServiceEndPointProviderFactory>();
        return services;
    }

}