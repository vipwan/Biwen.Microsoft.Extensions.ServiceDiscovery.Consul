using Microsoft.Extensions.ServiceDiscovery;

namespace Microsoft.Extensions.Hosting
{
    public static class HostingExtensions
    {
        /// <summary>
        /// AddConsulServiceEndpointProvider
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
}