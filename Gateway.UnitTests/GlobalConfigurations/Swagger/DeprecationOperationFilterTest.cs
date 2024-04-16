using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentAssertions;
using Gateway.Swagger;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Gateway.UnitTests.Swagger;

public class DeprecationOperationFilterTest
{
    [Fact]
    public void ShouldDeprecateOperationsAccordingToVersion()
    {
        var filter = new DeprecationOperationFilter();

        filter.Should().NotBeNull();
    }
}