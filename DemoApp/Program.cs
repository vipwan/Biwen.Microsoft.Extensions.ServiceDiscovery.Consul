﻿// Licensed to the DemoApp under one or more agreements.
// The DemoApp licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Consul.AspNetCore;
using System.Text.Json.Serialization;

Console.WriteLine($"Biwen.Microsoft.Extensions.ServiceDiscovery Version:{Biwen.Microsoft.Extensions.ServiceDiscovery.Generated.AssemblyMetadata.Version}");

var builder = WebApplication.CreateSlimBuilder(args);

//健康监测
builder.Services.AddHealthChecks().AddCheck("default", () =>
{
    return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, "healthy");
});

//注册 Consul服务和发现
builder.Services.AddConsul(o =>
{
    // Consul地址.如果非本地请修改
    o.Address = new("http://127.0.0.1:8500");
}).AddConsulServiceRegistration(registrationConfig =>
    {
        registrationConfig.Meta = new Dictionary<string, string>() { { "Weight", "1" } };
        registrationConfig.ID = "SVC1";
        registrationConfig.Port = 5124;
        registrationConfig.Name = "todo";
        registrationConfig.Address = "http://127.0.0.1";
        registrationConfig.Tags = ["MicroSvc"];
        registrationConfig.Check = new Consul.AgentServiceCheck
        {
            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(15),//服务启动多久后注册
            Interval = TimeSpan.FromSeconds(15),//健康检查时间间隔，或者称为心跳间隔
            HTTP = "http://127.0.0.1:5124/health",//健康检查地址
            Timeout = TimeSpan.FromSeconds(5),
            Method = "GET",
        };
    });


// 使用Microsoft.Extensions.ServiceDiscovery实现负载均衡
builder.Services.AddServiceDiscovery(o =>
{
    //设置Provider10秒刷新 用于测试所以频率有意调快
    o.RefreshPeriod = TimeSpan.FromSeconds(10d);
})
    .AddConfigurationServiceEndpointProvider() //config
    .AddConsulServiceEndpointProvider(); //consul

builder.Services.ConfigureHttpClientDefaults(static http =>
{
    http.AddServiceDiscovery();
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
