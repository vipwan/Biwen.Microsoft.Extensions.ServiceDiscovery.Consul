namespace Microsoft.Extensions.Hosting
{
    public static class HostingExtensions
    {
        /// <summary>
        /// AddConsulServiceEndPointResolver
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsulServiceEndPointResolver(this IServiceCollection services)
        {
            services.AddServiceDiscoveryCore();
            services.AddSingleton<IServiceEndPointResolverProvider, ConsulServiceEndPointResolverProvider>();
            return services;
        }
    }
}