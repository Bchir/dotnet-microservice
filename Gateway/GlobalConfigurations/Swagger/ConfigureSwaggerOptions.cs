using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gateway.Swagger;

public class ConfigureSwaggerOptions(
    IApiVersionDescriptionProvider provider,
    IOptions<ServiceMetadata> serviceMetaData
) : IConfigureOptions<SwaggerGenOptions>
{
    private const string BreakLine = "<br />";

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions.Reverse())
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var text = new StringBuilder(serviceMetaData.Value.Description);
        text.Append(BreakLine);
        text.Append($"[Link to BluePrint]({serviceMetaData.Value.BluePrintUri})");
        text.Append(BreakLine);

        var info = new OpenApiInfo()
        {
            Title = serviceMetaData.Value.ServiceName,
            Version = description.ApiVersion.ToString(),
            Contact = new OpenApiContact()
            {
                Name = serviceMetaData.Value.ContactName,
                Email = serviceMetaData.Value.ContactEmail
            },
            License = new OpenApiLicense()
            {
                Name = serviceMetaData.Value.License,
                Url = serviceMetaData.Value.LicenseUri
            },
            TermsOfService = serviceMetaData.Value.TermsOfService
        };

        if (description.IsDeprecated)
        {
            text.Append(BreakLine);
            text.Append("This API version has been deprecated.");
        }

        if (description.SunsetPolicy is SunsetPolicy policy)
        {
            if (policy.Date is DateTimeOffset when)
            {
                text.Append("The API will be sunset on ")
                    .Append(when.Date.ToShortDateString())
                    .Append('.');
            }

            if (policy.HasLinks)
            {
                text.Append(BreakLine);
                foreach (var link in policy.Links)
                {
                    text.Append($"[{link.Title}]({link.LinkTarget})");
                }
            }
        }
        text.Append(BreakLine);

        info.Description = text.ToString();
        return info;
    }
}