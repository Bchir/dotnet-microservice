using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gateway.Swagger;

public static class SwaggerDocumentation
{
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var serviceMetadata = app.Services.GetRequiredService<IOptions<ServiceMetadata>>();
                var descriptions = app.DescribeApiVersions();

                foreach (var description in descriptions.Reverse())
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = description.GroupName.ToUpperInvariant();
                    options.SwaggerEndpoint(url, name);
                }
                options.EnableFilter();
                options.DocumentTitle = serviceMetadata.Value.ServiceName;
                options.EnableValidator();
                options.EnableDeepLinking();
                options.ShowCommonExtensions();
                options.ShowExtensions();
                options.DisplayOperationId();
                options.DisplayRequestDuration();
            });
        }
        return app;
    }

    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddSwaggerGen(o =>
        {
            o.OperationFilter<DeprecationOperationFilter>();
            o.UseAllOfForInheritance();
            o.UseAllOfToExtendReferenceSchemas();
            o.UseInlineDefinitionsForEnums();
            o.UseOneOfForPolymorphism();
            o.SupportNonNullableReferenceTypes();
            o.DescribeAllParametersInCamelCase();
            o.OrderActionsBy(x => x.HttpMethod);
        });

        return builder;
    }
}