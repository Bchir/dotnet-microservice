namespace Gateway;

public class ServiceMetadata
{
    public required string ServiceName { get; set; }
    public Uri? BluePrintUri { get; set; }
    public required string Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactName { get; set; }
    public Uri? LicenseUri { get; set; }
    public required string License { get; set; }
    public Uri? TermsOfService { get; set; }
}

public static class ServiceMetadataRegistration
{
    public static ServiceMetadata AddServiceMetadata(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ServiceMetadata>(
            builder.Configuration.GetRequiredSection(nameof(ServiceMetadata))
        );
        var serviceMetadata = new ServiceMetadata()
        {
            ServiceName = string.Empty,
            Description = string.Empty,
            License = string.Empty
        };

        builder.Configuration.GetSection(nameof(ServiceMetadata)).Bind(serviceMetadata);
        return serviceMetadata;
    }
}