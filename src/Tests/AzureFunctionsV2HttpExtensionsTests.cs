using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NJsonSchema;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Processors;
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
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Query);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.String);
        }

        [Fact]
        public async Task Should_include_queryparam_of_type_int_list_that_is_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsQueryParams2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Name.Should().Be("queryParam");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Query);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.Array);
            operation.ActualParameters[0].Item.Type.Should().Be(JsonObjectType.Integer);
        }

        [Fact]
        public async Task Should_include_queryparam_of_type_Dog_that_is_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsQueryParams3);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Name.Should().Be("queryParam");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Query);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.Object);
            operation.ActualParameters[0].ActualSchema.Should().Be(swaggerDoc.Definitions["Dog"]);
        }


        [Fact]
        public async Task Should_include_header_of_type_string_that_is_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsHeaders1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Name.Should().Be("x-header");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Header);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.String);
        }

        [Fact]
        public async Task Should_include_header_of_type_Dog_that_is_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsHeaders2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Name.Should().Be("x-header");
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Header);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.Object);
            operation.ActualParameters[0].ActualSchema.Should().Be(swaggerDoc.Definitions["Dog"]);
        }

        [Fact]
        public async Task Should_include_body_of_type_Dog_that_is_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsBody1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Body);
            operation.ActualParameters[0].ActualSchema.Should().Be(swaggerDoc.Definitions["Dog"]);
        }

        [Fact]
        public async Task Should_include_body_of_type_string_that_is_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsBody2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Body);
            operation.ActualParameters[0].Schema.Type.Should().Be(JsonObjectType.String);
        }

        [Fact]
        public async Task Should_include_body_of_type_XmlDocument_and_consume_xml()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsBody4);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Body);
            operation.ActualParameters[0].Schema.Type.Should().Be(JsonObjectType.String);
            operation.ActualConsumes.Should().Contain("application/xml");
        }

        [Fact]
        public async Task Should_include_body_of_type_Dog_array_that_is_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsBody3);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Body);
            operation.ActualParameters[0].Schema.Type.Should().Be(JsonObjectType.Array);
            operation.ActualParameters[0].Schema.Item.ActualSchema.Should().Be(swaggerDoc.Definitions["Dog"]);
        }

        [Fact]
        public async Task Should_include_form_field_of_type_string_that_is_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsForm1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.FormData);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.String);
        }

        [Fact]
        public async Task Should_include_form_field_of_type_Dog_that_is_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsForm2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.FormData);
            operation.ActualParameters[0].ActualSchema.Should().Be(swaggerDoc.Definitions["Dog"]);
        }

        [Fact]
        public async Task Should_create_authorized_operation_from_HttpAuthorizeAttributed_function()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            settings.OperationProcessors.Add(new OperationSecurityProcessor("Bearer", OpenApiSecuritySchemeType.OAuth2));
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsJwtAuth1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(0); // HttpUser is not a HttpParam
            operation.Security.Count.Should().Be(1);
            operation.Security.First().Keys.Count.Should().Be(1);
        }

        [Fact]
        public async Task Should_create_authorized_operation_from_HttpAuthorizeAttribute_inheriting_attributed_function()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            settings.OperationProcessors.Add(new OperationSecurityProcessor("Bearer", OpenApiSecuritySchemeType.OAuth2));
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(HttpExtensionTests.HttpExtensionsJwtAuth2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(HttpExtensionTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(0); // HttpUser is not a HttpParam
            operation.Security.Count.Should().Be(1);
            operation.Security.First().Keys.Count.Should().Be(1);
        }
    }
}
