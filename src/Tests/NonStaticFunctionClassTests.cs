using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp;
using Xunit;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests
{
    public class NonStaticFunctionClassTests
    {
        [Fact]
        public async Task Should_include_nonstatic_function_classes_and_methods()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassesAsync(
                new[] {typeof(NonStaticFunctionClass), typeof(IgnoredNonStaticFunctionClass)}, null);

            // Assert
            swaggerDoc.Operations.Count().Should().Be(1);
        }

        [Fact]
        public async Task Should_find_nonstatic_function_classes()
        {
            // Arrange

            // Act
            var azureFuncClasses = AzureFunctionsV2ToSwaggerGenerator.GetAzureFunctionClasses(typeof(NonStaticFunctionClass).Assembly);

            // Assert
            azureFuncClasses.Should().Contain(x => x.Name == nameof(NonStaticFunctionClass));
        }
    }
}
