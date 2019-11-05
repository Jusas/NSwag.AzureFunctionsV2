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
    
    public class SwaggerAzureFunctionsV2AnnotationsTests
    {

        [Fact]
        public async Task Should_include_security_spec_in_SwaggerDocument_from_authorize_attribute_with_defaults()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            settings.OperationProcessors.Add(new OperationSecurityProcessor("Basic", OpenApiSecuritySchemeType.Basic));
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerAuthorizeAttribute1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() {functionName});

            // Assert
            Assert.Equal(1, swaggerDoc.Operations.First().Operation.ActualSecurity.Count);
        }

        [Fact]
        public async Task Should_include_security_spec_in_SwaggerDocument_from_authorize_attribute_with_scheme_apikey()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            settings.OperationProcessors.Add(new OperationSecurityProcessor("HdrApiKey", OpenApiSecuritySchemeType.ApiKey));
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerAuthorizeAttribute2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            Assert.Equal(1, operation.ActualSecurity.Count);
            operation.ActualSecurity.First().Should().ContainKey("HdrApiKey");
        }

        [Fact]
        public async Task Should_include_security_spec_in_SwaggerDocument_from_authorize_attribute_with_scheme_apikey_in_query()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            settings.OperationProcessors.Add(new OperationSecurityProcessor("QApiKey", OpenApiSecuritySchemeType.ApiKey, OpenApiSecurityApiKeyLocation.Query));
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerAuthorizeAttribute3);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            Assert.Equal(1, operation.ActualSecurity.Count);
            operation.ActualSecurity.First().Should().ContainKey("QApiKey");
        }

        [Fact]
        public async Task Should_include_required_formdata_field_with_type_string()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerFormDataAttribute1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be("description");
            operation.ActualParameters[0].Name.Should().Be("formField1");
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Type.Should().Be(NJsonSchema.JsonObjectType.String);
        }

        [Fact]
        public async Task Should_include_formdatafile_field()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerFormDataFileAttribute1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be("description");
            operation.ActualParameters[0].Name.Should().Be("file");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.FormData);
            operation.ActualParameters[0].Type.Should().Be(NJsonSchema.JsonObjectType.File);
        }

        [Fact]
        public async Task Should_include_formdatafile_field_with_multifile()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerFormDataFileAttribute2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be("description");
            operation.ActualParameters[0].Name.Should().Be("files");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.FormData);
            operation.ActualParameters[0].Type.Should().Be(NJsonSchema.JsonObjectType.File);
            operation.ActualParameters[0].CollectionFormat.Should().Be(OpenApiParameterCollectionFormat.Multi);
        }

        [Fact]
        public async Task Should_include_query_param_with_string_Type_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerQueryParamAttribute1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be("A query parameter");
            operation.ActualParameters[0].Name.Should().Be("queryParam");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Query);
            operation.ActualParameters[0].Type.Should().Be(NJsonSchema.JsonObjectType.String);
        }

        [Fact]
        public async Task Should_include_query_param_with_int_Type_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerQueryParamAttribute2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be("Yet another description");
            operation.ActualParameters[0].Name.Should().Be("queryParam");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Query);
            operation.ActualParameters[0].Type.Should().Be(NJsonSchema.JsonObjectType.Integer);
        }

        [Fact]
        public async Task Should_include_query_param_with_int_list_Type_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerQueryParamAttribute3);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be(null);
            operation.ActualParameters[0].Name.Should().Be("queryParam");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Query);
            operation.ActualParameters[0].Type.Should().Be(NJsonSchema.JsonObjectType.Array);
            operation.ActualParameters[0].Item.Type.Should().Be(NJsonSchema.JsonObjectType.Integer);
        }

        [Fact]
        public async Task Should_include_body_of_type_Person_that_is_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerRequestBodyTypeAttribute1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be("description");
            operation.ActualParameters[0].Name.Should().Be("Body");
            operation.ActualParameters[0].IsRequired.Should().Be(true);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Body);
            operation.ActualParameters[0].ActualSchema.Type.Should().Be(JsonObjectType.Object);
            operation.ActualParameters[0].ActualSchema.Should().Be(swaggerDoc.Definitions["Person"]);
        }

        [Fact]
        public async Task Should_include_header_of_type_string_that_is_not_required()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerRequestHeaderAttribute1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            var operation = swaggerDoc.Operations.First().Operation;
            operation.ActualParameters.Count.Should().Be(1);
            operation.ActualParameters[0].Description.Should().Be("description");
            operation.ActualParameters[0].Name.Should().Be("x-header");
            operation.ActualParameters[0].IsRequired.Should().Be(false);
            operation.ActualParameters[0].Kind.Should().Be(OpenApiParameterKind.Header);
            operation.ActualParameters[0].Type.Should().Be(JsonObjectType.String);
        }

        [Fact]
        public async Task Should_ignore_SwaggerIgnore_attributed_function()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerIgnoredFunction1);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            swaggerDoc.Operations.Count().Should().Be(0);
        }

        [Fact]
        public async Task Should_ignore_OpenApiIgnore_attributed_function()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerIgnoredFunction2);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            swaggerDoc.Operations.Count().Should().Be(0);
        }

        [Fact]
        public async Task Should_not_limit_to_HttpRequest_class_in_discovery()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerFunctionWithNonHttpRequestParam);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            swaggerDoc.Operations.Count().Should().Be(1);
        }

        [Fact]
        public async Task Should_have_body_type_automatically_discovered_from_custom_post_request_body_type()
        {
            // Arrange
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var functionName = nameof(GenerationAnnotationTests.SwaggerFunctionWithNonHttpRequestParam);

            // Act
            var swaggerDoc = await generator.GenerateForAzureFunctionClassAndSpecificMethodsAsync(
                typeof(GenerationAnnotationTests), new List<string>() { functionName });

            // Assert
            swaggerDoc.Operations.Count().Should().Be(1);
            swaggerDoc.Operations.First().Operation.RequestBody.Name.Should().Be("req");
        }

    }
}
