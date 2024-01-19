using Consul.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.ServiceDiscovery.Abstractions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

//健康监测
builder.Services.AddHealthChecks().AddCheck("test", () =>
{
    return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, "健康");
});

//注册 Consul服务和发现
builder.Services.AddConsul().AddConsulServiceRegistration(cfg =>
    {
        cfg.Meta = new Dictionary<string, string>() { { "Weight", "1" } };
        cfg.ID = "SVC1";
        cfg.Port = 5124;
        cfg.Name = "todo";
        cfg.Address = "http://127.0.0.1";
        cfg.Tags = ["MicroSvc"];
        cfg.Check = new Consul.AgentServiceCheck
        {
            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(15),//服务启动多久后注册
            Interval = TimeSpan.FromSeconds(15),//健康检查时间间隔，或者称为心跳间隔
            HTTP = "http://127.0.0.1:5124/health",//健康检查地址
            Timeout = TimeSpan.FromSeconds(5),
            Method = "GET",
        };
    });


// 使用Microsoft.Extensions.ServiceDiscovery实现负载均衡
builder.Services.AddServiceDiscovery().AddConsulServiceEndPointResolver();

// 默认是轮询算法,当前包含了
// RoundRobinServiceEndPointSelectorProvider轮询,
// RandomServiceEndPointSelectorProvider随即,
// PickFirstServiceEndPointSelectorProvider第一个.
// PowerOfTwoChoicesServiceEndPointSelectorProvider选择负载最轻的端点进行分布式负载均衡，当提供的任何一个端点不具备该功能时，降级为随机选择端点

builder.Services.ConfigureHttpClientDefaults(static http =>
{
    // 可以使用自己的算法 下面使用随机算法
    http.UseServiceDiscovery(RandomServiceEndPointSelectorProvider.Instance);
});

//使用IHttpClientFactory
builder.Services.AddHttpClient("todo", cfg =>
{
    cfg.BaseAddress = new("http://todo");
});


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();


// Consul健康监测
app.UseHealthChecks("/health");


#region apis

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

#endregion


#region 测试服务发现和负载


app.MapGet("/test", async (IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("todo");
    var response = await client.GetAsync("/todos");
    var todos = await response.Content.ReadAsStringAsync();
    return Results.Content(todos, contentType: "application/json");
});


#endregion



app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
