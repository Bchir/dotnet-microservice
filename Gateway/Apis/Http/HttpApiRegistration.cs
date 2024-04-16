using Asp.Versioning.Builder;

namespace Gateway.Apis.Http;

public static class HttpApiRegistration
{
    public static WebApplication UseHttpApis(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        app.MapGet("v{version:apiVersion}/{id:int}", (int id) => id)
            .ProducesValidationProblem()
            .Produces(200)
            .Produces(404)
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(2.0)
            .WithOpenApi();

        app.MapGet(
                "v{version:apiVersion}/single/{id:int}",
                (int id) =>
                {
                    throw new Exception($"Oups {id}");
                }
            )
            .ProducesValidationProblem()
            .Produces(200)
            .Produces(404)
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(1.0)
            .WithOpenApi();
        return app;
    }
}