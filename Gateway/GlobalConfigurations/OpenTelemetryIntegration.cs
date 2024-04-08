using HealthChecks.OpenTelemetry.Instrumentation;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

namespace Gateway;

public static partial class OpenTelemetryIntegration
{
    public static WebApplicationBuilder AddOpenTelemetryIntegration(this WebApplicationBuilder builder, ServiceMetadata serviceMetadata)
    {
        builder.Services.Configure<OpenTelemetryOptions>(builder.Configuration);
        builder.Services.Configure<HealthChecksInstrumentationOptions>(options =>
        {
            options.StatusGaugeName = serviceMetadata.ServiceName;
            options.DurationGaugeName = serviceMetadata.ServiceName + "_duration";
        });

        var openTelemetryOptions = new OpenTelemetryOptions();
        builder.Configuration.Bind(openTelemetryOptions);
        builder.Services
            .AddOpenTelemetry()
            .AddTracing(serviceMetadata, openTelemetryOptions)
            .AddMetrics(serviceMetadata, openTelemetryOptions);
        return builder;
    }

    private static IOpenTelemetryBuilder AddTracing(
        this IOpenTelemetryBuilder builder,
        ServiceMetadata serviceMetadata,
        OpenTelemetryOptions config)
    {
        builder.WithTracing(tracing =>
            {
                tracing
                     .AddSource()
                     .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceMetadata.ServiceName))
                     .SetErrorStatusOnException()
                     .SetSampler(new AlwaysOnSampler())
                     .AddAspNetCoreInstrumentation(options =>
                     {
                         options.RecordException = true;
                     })
                     .AddGrpcClientInstrumentation()
                     .AddHttpClientInstrumentation(o =>
                        {
                            o.RecordException = true;
                        });
                builder.Services.ConfigureOpenTelemetryTracerProvider((tracing) =>
                {
                    if (config.Enabled)
                        tracing.AddOtlpExporter(x =>
                        {
                            x.Headers = config.Headers;
                            x.Endpoint = config.Endpoint!;
                            x.TimeoutMilliseconds = config.TimeoutMilliseconds;
                            x.Protocol = config.Protocol;
                        });
                });
            });

        return builder;
    }

    private static IOpenTelemetryBuilder AddMetrics(
        this IOpenTelemetryBuilder builder,
        ServiceMetadata serviceMetadata,
        OpenTelemetryOptions config)
    {
        builder.WithMetrics(metrics =>
        {
            var meter = new Meter(serviceMetadata.ServiceName);

            metrics
                     .AddMeter(meter.Name)
                     .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(meter.Name))
                     .AddAspNetCoreInstrumentation()
                     .AddRuntimeInstrumentation()
                     .AddHttpClientInstrumentation()
                     .AddEventCountersInstrumentation()
                     .AddProcessInstrumentation()
                     .AddHealthChecksInstrumentation();

            builder.Services.ConfigureOpenTelemetryMeterProvider((metrics) =>
            {
                if (config.Enabled)
                    metrics.AddOtlpExporter(x =>
                    {
                        x.Headers = config.Headers;
                        x.Endpoint = config.Endpoint!;
                        x.TimeoutMilliseconds = config.TimeoutMilliseconds;
                        x.Protocol = config.Protocol;
                    });
            });
        });

        return builder;
    }
}