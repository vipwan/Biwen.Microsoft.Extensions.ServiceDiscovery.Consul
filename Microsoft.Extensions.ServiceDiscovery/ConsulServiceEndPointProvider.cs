namespace Biwen.Microsoft.Extensions.ServiceDiscovery;

internal class ConsulServiceEndPointProvider(ServiceEndpointQuery query, IConsulClient consulClient, ILogger logger)
    : IServiceEndpointProvider, IHostNameFeature
{
    const string Name = "Consul";
    private readonly string _serviceName = query.ServiceName;
    private readonly IConsulClient _consulClient = consulClient;
    private readonly ILogger _logger = logger;

    public string HostName => query.ServiceName;

    public ValueTask DisposeAsync() => default;

    public async ValueTask PopulateAsync(IServiceEndpointBuilder endPoints, CancellationToken cancellationToken)
    {
        var flag = ServiceNameParts.TryParse(_serviceName, out var serviceNameParts);
        var sum = 0;
        if (flag)
        {
            var queryResult = await _consulClient.Health.Service(serviceNameParts.Host, string.Empty, true, cancellationToken);

            if (queryResult.Response is null)
            {
                _logger.LogError($"The Consul server request is abnormal, please confirm that the service is available!");
                return;
            }

            foreach (var serviceEntry in queryResult.Response)
            {
                var address = $"{serviceEntry.Service.Address}:{serviceEntry.Service.Port}";
                var isEndpoint = ServiceNameParts.TryCreateEndPoint(address, out var endPoint);
                if (isEndpoint)
                {
                    ++sum;
                    var serviceEndPoint = ServiceEndpoint.Create(endPoint!);
                    serviceEndPoint.Features.Set<IServiceEndpointProvider>(this);
                    serviceEndPoint.Features.Set<IHostNameFeature>(this);
                    endPoints.Endpoints.Add(serviceEndPoint);
                    _logger.LogInformation("ConsulServiceEndPointProvider Found Service {_serviceName}:{address}", _serviceName, address);
                }
            }
        }

        if (sum == 0)
        {
            _logger.LogWarning("No ConsulServiceEndPointProvider were found for service '{_serviceName}' ('{HostName}').", _serviceName, HostName);
        }
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}