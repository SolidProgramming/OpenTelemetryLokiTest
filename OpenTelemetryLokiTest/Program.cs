using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryLokiTest;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.AddOpenTelemetry(_ =>
{
    _.AddConsoleExporter();
});

builder.Services.AddHttpClient();

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(nameof(SomeService)))
        .AddConsoleExporter();
});

builder.Services.AddOpenTelemetry()
     .ConfigureResource(resource => resource.AddService(nameof(SomeService)))
    .WithTracing(tracing =>
        tracing.AddAspNetCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics =>
        metrics.AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

builder.Services.AddHealthChecks()
    .AddCheck<PublicIPHealthCheck>("Public IP-Check", tags: ["ip", "network", "external"], timeout: TimeSpan.FromSeconds(3));

builder.Services.AddHttpLogging(_ =>
{
    _.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});
builder.Services.AddSingleton<ISomeService, SomeService>();
// Add services to the container.

WebApplication app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/logsomething", (ISomeService someService) =>
{
    someService.LogSomething();
});


app.Run();