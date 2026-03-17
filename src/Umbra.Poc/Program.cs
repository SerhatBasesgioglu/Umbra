using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();
        builder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
