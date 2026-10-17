using Brasa.Application.Shared.Observability;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Brasa.Api.Observability;

internal static class ObservabilityRegistration
{
    public const string DefaultServiceName = "brasa-api";
    public const string ServiceNameSetting = "OTEL_SERVICE_NAME";
    public const string ExporterEndpointSetting = "OTEL_EXPORTER_OTLP_ENDPOINT";

    private static readonly string[] Meters =
    [
        BrasaMetrics.MeterName,
        "System.Runtime",
        "Npgsql",
        "Microsoft.AspNetCore.Http.Connections",
        "Microsoft.AspNetCore.RateLimiting",
    ];

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var telemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Configuration[ServiceNameSetting] ?? DefaultServiceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.Filter = http => !http.Request.Path.StartsWithSegments("/api/v1/health"))
                .AddHttpClientInstrumentation()
                .AddSource("Npgsql"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(Meters));

        if (!string.IsNullOrWhiteSpace(builder.Configuration[ExporterEndpointSetting]))
        {
            telemetry.UseOtlpExporter();
        }

        return builder;
    }
}
