using Gateway.Swagger;

namespace Gateway;

public class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder
            .Configuration
            .AddEnvironmentVariables();

        var serviceMetaData = builder
            .AddServiceMetadata();

        builder
            .AddVersioning()
            .AddSwagger()
            .AddLogger();
        // security reasons
        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
        builder.AddOpenTelemetryIntegration(serviceMetaData);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails();
        builder.AddApis();

        var app = builder.Build();
        app.UseRequestLogging();
        app.UseStatusCodePages();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseProbes();

        app.UseSwaggerDocumentation();
        app.UseApis();
        await app.RunAsync();
    }
}