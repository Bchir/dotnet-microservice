using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenTelemetry;
using System.Diagnostics;
using static Gateway.OpenTelemetryIntegration;
using MicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace Gateway;

public static class Logging
{
    private static readonly Action<MicrosoftLogger, string, string, int?, TimeSpan, Exception?> _logCompletion =
        LoggerMessage.Define<string, string, int?, TimeSpan>(
            LogLevel.Information,
            new EventId(1, nameof(Logging)),
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed} ms"
        );

    public static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                await next.Invoke();
                stopWatch.Stop();
                LogCompletion(context, stopWatch.Elapsed);
            }
            catch (Exception e)
            {
                if (stopWatch.IsRunning)
                    stopWatch.Stop();
                LogCompletion(context, stopWatch.Elapsed, e);
                throw;
            }
        });
        return app;
    }

    public static WebApplicationBuilder AddLogger(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((_, sp, configuration) =>
        {
            EnrichLog(configuration, sp);
            SinkLog(configuration, sp);
        });

        return builder;
    }

    private static void SinkLog(LoggerConfiguration logConfiguration, IServiceProvider serviceProvider)
    {
        var serviceMetaData = serviceProvider.GetRequiredService<IOptions<ServiceMetadata>>();
        var openTelemetryOptions = serviceProvider.GetRequiredService<IOptions<OpenTelemetryOptions>>();

        const string LogTemplate = "[{Timestamp:HH:mm:ss}][{Level:u3}][{SourceContext:1}] {Message:lj}{NewLine}{Exception}";
        logConfiguration
        .WriteTo.Console(outputTemplate: LogTemplate);
        if (openTelemetryOptions.Value.Enabled)
        {
            logConfiguration.WriteTo.OpenTelemetry(cfg =>
             {
                 cfg.Endpoint = $"{openTelemetryOptions.Value.Endpoint}/v1/logs";
                 cfg.Headers = openTelemetryOptions.Value.ParsedHeaders;
                 cfg.Protocol = MapProtocol(openTelemetryOptions.Value.Protocol);
                 cfg.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField;
                 cfg.ResourceAttributes = new Dictionary<string, object>
                                                {
                                                    {"service.name", serviceMetaData.Value.ServiceName},
                                                    {"index", 10},
                                                    {"flag", true},
                                                    {"value", 3.14}
                                                };
             });
        }
    }

    private static OtlpProtocol MapProtocol(OtlpExportProtocol protocol)
    {
        return protocol switch
        {
            OtlpExportProtocol.Grpc => OtlpProtocol.Grpc,
            OtlpExportProtocol.HttpProtobuf => OtlpProtocol.HttpProtobuf,
            _ => OtlpProtocol.Grpc,
        };
    }

    private static void EnrichLog(LoggerConfiguration configuration, IServiceProvider serviceProvider)
    {
        var serviceMetaData = serviceProvider
            .GetRequiredService<IOptions<ServiceMetadata>>();

        configuration
           .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
           .Enrich.WithProperty(nameof(serviceMetaData.Value.ServiceName), serviceMetaData.Value.ServiceName)
           .Enrich.WithExceptionDetails()
           .Enrich.FromLogContext()
           .Enrich.WithThreadId()
           .Enrich.WithThreadName()
           .Enrich.WithProcessName()
           .Enrich.WithProcessId()
           .Enrich.WithEnvironmentName()
           .Enrich.WithMachineName()
           .Enrich.FromGlobalLogContext()
           .Enrich.WithClientIp()
           .Enrich.WithSensitiveDataMasking((options) =>
            {
                options.MaskingOperators =
                [
                    new EmailAddressMaskingOperator(),
                    new IbanMaskingOperator(),
                    new CreditCardMaskingOperator()
                ];
            })
           .Enrich.WithSpan(new SpanOptions
           {
               IncludeBaggage = true,
               IncludeOperationName = true,
               IncludeTags = true
           });
    }

    private static string GetPath(HttpContext httpContext)
    {
        var requestPath = httpContext.Features.Get<IHttpRequestFeature>()?.Path;
        if (string.IsNullOrEmpty(requestPath))
        {
            requestPath = httpContext.Request.Path.ToString();
        }

        return requestPath!;
    }

    private static void LogCompletion(HttpContext httpContext, TimeSpan elapsed, Exception? exception = null)
    {
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<ApiLogging>>();
        var method = httpContext.Request.Method;
        var path = GetPath(httpContext);
        var statusCode = httpContext.Response?.StatusCode;
        _logCompletion(logger, method, path, statusCode, elapsed, exception);
    }

    private record ApiLogging();
}