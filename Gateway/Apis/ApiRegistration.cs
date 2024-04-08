using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Gateway;

public record VersionState(ApiVersion Version, bool IsDeprecated);

public static class ApiRegistration
{
    public static readonly VersionState VersionOne = new(new(1.0), true);
    public static readonly VersionState VersionTwo = new(new(2.0), false);

    public static WebApplication UseApis(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .AddVersion(VersionOne)
            .AddVersion(VersionTwo)
            .Build();

        app.UseHttpApis(versionSet);

        return app;
    
    }


    public static WebApplicationBuilder AddApis(this WebApplicationBuilder builder)
    {
        // builder.AddAsyncApiHost();
        return builder;
    }

    public static ApiVersionSetBuilder AddVersion(this ApiVersionSetBuilder apiVersionSetBuilder, VersionState versionState)
    {
        if (versionState.IsDeprecated)
        {
            apiVersionSetBuilder
                .HasDeprecatedApiVersion(versionState.Version)
                .AdvertisesDeprecatedApiVersion(versionState.Version);
        }
        else
        {
            apiVersionSetBuilder
                        .HasApiVersion(versionState.Version)
                        .AdvertisesApiVersion(versionState.Version);
        }
        return apiVersionSetBuilder;
    }
}