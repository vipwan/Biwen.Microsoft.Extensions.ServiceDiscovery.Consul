namespace Biwen.Microsoft.Extensions.ServiceDiscovery
{

    public class ConsulServiceEndPointResolver(string serviceName, IConsulClient consulClient) : IServiceEndPointResolver //, IHostNameFeature
    {

        const string Name = "Consul";

        private readonly string _serviceName = serviceName;

        private readonly IConsulClient _consulClient = consulClient;

        public string HostName => _serviceName;

        public string DisplayName => Name;

#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize
        public ValueTask DisposeAsync() => default;
#pragma warning restore CA1816 // Dispose 方法应调用 SuppressFinalize

        public async ValueTask<ResolutionStatus> ResolveAsync(ServiceEndPointCollectionSource endPoints, CancellationToken cancellationToken)
        {
            var flag = ServiceNameParts.TryParse(endPoints.ServiceName, out var serviceNameParts);
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

                if (endPoints.EndPoints.Count == 0)
                {
                    return new ResolutionStatus(ResolutionStatusCode.NotFound, null, null!);
                }
                return new ResolutionStatus(ResolutionStatusCode.Success, null, null!);
            }
            // 转换失败的情况
            return new ResolutionStatus(ResolutionStatusCode.Error, null, null!);
        }

        public override string ToString() => Name;
    }
}