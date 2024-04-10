using Microsoft.Extensions.ServiceDiscovery;

namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{
    internal class ConsulServiceEndPointProvider(ServiceEndPointQuery query, IConsulClient consulClient) : IServiceEndPointProvider //, IHostNameFeature
    {

        const string Name = "Consul";

        private readonly string _serviceName = query.ServiceName;

        private readonly IConsulClient _consulClient = consulClient;

        public string HostName => _serviceName;

#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize
        public ValueTask DisposeAsync() => default;

        public async ValueTask PopulateAsync(IServiceEndPointBuilder endPoints, CancellationToken cancellationToken)
        {

            var flag = ServiceNameParts.TryParse(_serviceName, out var serviceNameParts);
            if (flag)
            {
                var queryResult = await _consulClient.Health.Service(serviceNameParts.Host, string.Empty, true, cancellationToken);
                foreach (var serviceEntry in queryResult.Response)
                {
                    var isEndpoint = ServiceNameParts.TryCreateEndPoint($"{serviceEntry.Service.Address}:{serviceEntry.Service.Port}",
                        out var endPoint);

                    if (isEndpoint)
                    {
                        endPoints.EndPoints.Add(ServiceEndPoint.Create(endPoint!));
                    }
                }
            }
        }

        public override string ToString() => Name;
    }
}