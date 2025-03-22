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

// Configure the HTTP request pipeline.

string[] summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ISomeService someService) =>
{
    WeatherForecast[] forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    someService.LogSomething();
    return forecast;
});

app.MapHealthChecks("/health");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)( TemperatureC / 0.5556 );
}
