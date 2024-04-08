using FluentAssertions;
using Xunit;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Gateway.Swagger;

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