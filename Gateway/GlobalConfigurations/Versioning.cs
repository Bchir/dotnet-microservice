using Asp.Versioning;

namespace Gateway;

public static class Versioning
{
    public static WebApplicationBuilder AddVersioning(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ReportApiVersions = true;
                options
                    .Policies.Sunset(1.0)
                    .Effective(DateTimeOffset.Now.AddDays(60))
                    .Link("https://github.com/bchir")
                    .Title("Versioning Policy")
                    .Type("text/html");
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            })
            .EnableApiVersionBinding();

        return builder;
    }
}