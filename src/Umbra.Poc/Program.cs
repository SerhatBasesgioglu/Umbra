using OpenTelemetry.Metrics;
using Umbra.Poc.Dump;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder
    .Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();
        builder.AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel",
            "Umbra.Poc.Ado"
        );
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<AdoMetrics>();
builder.Services.AddSingleton<PipelineFetcher>();
builder.Services.AddSingleton<WorkItemFetcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PipelineFetcher>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<WorkItemFetcher>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPrometheusScrapingEndpoint();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
