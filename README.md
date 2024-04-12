# Microsoft.Extensions.ServiceDiscovery集成Consul



## 使用方式

### 1.安装Nuget包

```shell

dotnet add package Biwen.Microsoft.Extensions.ServiceDiscovery.Consul --prerelease

```

### 2 Enjoy

`builder.Services.AddServiceDiscovery().AddConsulServiceEndpointProvider();`


```csharp

using Consul.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.ServiceDiscovery.Abstractions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

//健康监测
builder.Services.AddHealthChecks().AddCheck("default", () =>
{
    return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, "Healthy");
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


// 使用Microsoft.Extensions.ServiceDiscovery实现负载均衡 & Consul
builder.Services.AddServiceDiscovery().AddConsulServiceEndpointProvider();

builder.Services.ConfigureHttpClientDefaults(static http =>
{
    http.UseServiceDiscovery();
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


```
### 3 More

[Microsoft.Extensions.ServiceDiscovery](https://github.com/dotnet/aspire/tree/main/src/Microsoft.Extensions.ServiceDiscovery)

