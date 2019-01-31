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
    public class AzureFunctionsV2HttpExtensionsTests
    {

        [Fact]
        public async Task Should_include_queryparam_qp_that_is_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsQueryParams1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Name.Should().Be("qp");
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Kind.Should().Be(SwaggerParameterKind.Query);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.String);
        }

        // TODO the rest
    }
}
