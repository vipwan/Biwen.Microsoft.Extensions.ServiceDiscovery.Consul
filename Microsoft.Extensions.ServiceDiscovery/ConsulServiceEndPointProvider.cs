using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;

namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{
    internal class ConsulServiceEndPointProvider(ServiceEndPointQuery query, IConsulClient consulClient, ILogger logger) : IServiceEndPointProvider //, IHostNameFeature
    {
        const string Name = "Consul";
        private readonly string _serviceName = query.ServiceName;
        private readonly IConsulClient _consulClient = consulClient;
        private readonly ILogger _logger = logger;

        public string HostName => _serviceName;

#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize
        public ValueTask DisposeAsync() => default;

        public async ValueTask PopulateAsync(IServiceEndPointBuilder endPoints, CancellationToken cancellationToken)
        {
            var flag = ServiceNameParts.TryParse(_serviceName, out var serviceNameParts);
            var sum = 0;
            if (flag)
            {
                var queryResult = await _consulClient.Health.Service(serviceNameParts.Host, string.Empty, true, cancellationToken);
                foreach (var serviceEntry in queryResult.Response)
                {
                    var address = $"{serviceEntry.Service.Address}:{serviceEntry.Service.Port}";
                    var isEndpoint = ServiceNameParts.TryCreateEndPoint(address, out var endPoint);
                    if (isEndpoint)
                    {
                        ++sum;
                        endPoints.EndPoints.Add(ServiceEndPoint.Create(endPoint!));
                        _logger.LogInformation($"ConsulServiceEndPointProvider Found Service {_serviceName}:{address}");
                    }
                }
            }

            if (sum == 0)
            {
                _logger.LogWarning($"ConsulServiceEndPointProvider Found Service {_serviceName} not found");
            }
        }

        public override string ToString() => Name;
    }
}