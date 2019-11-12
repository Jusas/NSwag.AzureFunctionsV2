using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NJsonSchema;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp;
using Xunit;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests
{
   public class RouteParamTests
    {
        [Fact]
        public async Task Should_include_two_route_params_from_function_signature()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionsTestApp.RouteParamTests.RouteParamTest);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionsTestApp.RouteParamTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(2);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Path);
            operation.ActualParameters[1].Kind.Should().Be(OpenApiParameterKind.Path);
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[1].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.Integer);
            operation.ActualParameters[1].Type.Should().Be(JsonObjectType.String);
        }
    }
}
