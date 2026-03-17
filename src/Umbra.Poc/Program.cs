using OpenTelemetry.Metrics;

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
