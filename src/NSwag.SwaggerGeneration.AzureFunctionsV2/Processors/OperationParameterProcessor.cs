using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using System.Reflection;
using System.Text.RegularExpressions;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Infrastructure;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Processors
{
    public class OperationParameterProcessor : IOperationProcessor
    {
        /// <summary>
        /// A parameter that comes from method attributes such as SwaggerRequestHeaderAttribute.
        /// These parameters are not present in the method signature and only exist as attributes.
        /// </summary>
        private struct SwaggerMethodAttributeParameter
        {
            public SwaggerMethodAttributeParameter(string name, SwaggerMethodAttributeParameterType parameterType, Type explicitType, bool required,
                string description = null, IDictionary<string, object> properties = null)
            {
                Name = name;
                ParameterType = parameterType;
                ExplicitType = explicitType;
                Description = description;
                Required = required;
                Properties = properties != null ? new Dictionary<string, object>(properties) : null;
            }

            public string Name { get; }
            public SwaggerMethodAttributeParameterType ParameterType { get; }
            public Type ExplicitType { get; }
            public bool Required { get; }
            public string Description { get; }
            public Dictionary<string, object> Properties { get; }
        }

        private enum SwaggerMethodAttributeParameterType
        {
            File,
            TypedBody,
            Header,
            Query,
            Form
        }

        /// <summary>
        /// A dummy interface for file uploads, necessary for JSON file type
        /// resolving.
        /// </summary>
        public interface IFormFile
        {
        }

        private AzureFunctionsV2ToSwaggerGeneratorSettings _settings;

        public OperationParameterProcessor(AzureFunctionsV2ToSwaggerGeneratorSettings settings)
        {
            _settings = settings;
        }

        private bool HasBindingAttribute(ParameterInfo parameter)
        {
            // Support for AzureFunctionsV2.HttpExtensions.
            var allowedBindingAttributes = new string[]
            {
                "HttpQueryAttribute",
                "HttpFormAttribute",
                "HttpBodyAttribute",
                "HttpHeaderAttribute"
            };

            foreach(var attribute in parameter.GetCustomAttributes())
            {
                var attributeClassAttributes = attribute.GetType().GetCustomAttributes();
                if (attributeClassAttributes.Any(a => a.GetType().Name == "BindingAttribute" &&
                    !allowedBindingAttributes.Contains(attribute.GetType().Name)))
                    return true;
            }

            return false;
        }

        public async Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var httpPath = context.OperationDescription.Path;
            var position = 1;

            var ignoredParameterTypeNames = new string[] {"HttpRequest", "TraceWriter", "TextWriter", "ILogger"}; // Microsoft.Azure.WebJobs.Host.TraceWriter Microsoft.Extensions.Logging.ILogger
            var ignoreAttributeTypeNames = new string[] {"SwaggerIgnoreAttribute", "FromServicesAttribute", "BindNeverAttribute"};

            // Ignore parameters that are directly Azure Functions related or explicitly marked as ignored.

            var parameters = new List<object>(
                context.MethodInfo.GetParameters()
                    .Where(x => !ignoredParameterTypeNames.Contains(x.ParameterType.Name) && 
                                x.GetCustomAttributes().All(a => !ignoreAttributeTypeNames.Contains(a.GetType().Name)) &&
                                !HasBindingAttribute(x))
                );

            // Add HTTP request body (Post, Put) from SwaggerRequestBodyTypeAttribute if it's present.
            // Otherwise we are unable to determine the body type.

            var requestBodyTypeAttribute = context.MethodInfo.GetCustomAttributes()
                .SingleOrDefault(x => x.GetType().Name == "SwaggerRequestBodyTypeAttribute");            
            var headerAttributes = context.MethodInfo.GetCustomAttributes()
                .Where(x => x.GetType().Name == "SwaggerRequestHeaderAttribute").ToList();
            var queryAttributes = context.MethodInfo.GetCustomAttributes()
                .Where(x => x.GetType().Name == "SwaggerRequestHeaderAttribute").ToList();
            var uploadFileAttribute = context.MethodInfo.GetCustomAttributes()
                .SingleOrDefault(x => x.GetType().Name == "SwaggerFormDataFileAttribute");
            var formDataAttributes = context.MethodInfo.GetCustomAttributes()
                .Where(x => x.GetType().Name == "SwaggerFormDataAttribute").ToList();

            if (uploadFileAttribute != null)
            {
                var formDataFileParamName = uploadFileAttribute.TryGetPropertyValue("Name", default(string));
                var formDataFileParamDescription = uploadFileAttribute.TryGetPropertyValue("Description", default(string));
                var formDataFileMultiFile = uploadFileAttribute.TryGetPropertyValue("MultiFile", default(bool));
                parameters.Add(new SwaggerMethodAttributeParameter(formDataFileParamName, SwaggerMethodAttributeParameterType.File,
                    null, true, formDataFileParamDescription, new Dictionary<string, object>() {{"MultiFile", formDataFileMultiFile}}));
            }
            else if (requestBodyTypeAttribute != null)
            {
                var requestBodyParamType = requestBodyTypeAttribute.TryGetPropertyValue("Type", default(Type));
                var requestBodyParamDescription = requestBodyTypeAttribute.TryGetPropertyValue("Description", default(string));
                var requestBodyParamName = requestBodyTypeAttribute.TryGetPropertyValue("Name", default(string));
                var requestBodyParamRequired = requestBodyTypeAttribute.TryGetPropertyValue("Required", false);
                if (string.IsNullOrEmpty(requestBodyParamName))
                    requestBodyParamName = "Body";
                parameters.Add(new SwaggerMethodAttributeParameter(requestBodyParamName, SwaggerMethodAttributeParameterType.TypedBody, 
                    requestBodyParamType, requestBodyParamRequired, requestBodyParamDescription));
            }

            if (headerAttributes.Any())
            {
                foreach (var headerAttribute in headerAttributes)
                {
                    var headerName = headerAttribute.TryGetPropertyValue("Name", default(string));
                    var headerDescription = headerAttribute.TryGetPropertyValue("Description", default(string));
                    var headerType = headerAttribute.TryGetPropertyValue("Type", default(Type));
                    var headerRequired = headerAttribute.TryGetPropertyValue("Required", false);
                    if (headerType != null && headerName != null)
                        parameters.Add(new SwaggerMethodAttributeParameter(headerName, SwaggerMethodAttributeParameterType.Header, 
                            headerType, headerRequired, headerDescription));
                }
            }

            if (queryAttributes.Any())
            {
                foreach (var queryAttribute in queryAttributes)
                {
                    var queryParameterName = queryAttribute.TryGetPropertyValue("Name", default(string));
                    var queryParameterDescription = queryAttribute.TryGetPropertyValue("Description", default(string));
                    var queryParameterType = queryAttribute.TryGetPropertyValue("Type", default(Type));
                    var queryParameterRequired = queryAttribute.TryGetPropertyValue("Required", false);
                    if(queryParameterType != null && queryParameterName != null)
                        parameters.Add(new SwaggerMethodAttributeParameter(queryParameterName, SwaggerMethodAttributeParameterType.Query,
                            queryParameterType, queryParameterRequired, queryParameterDescription));
                }
            }

            if (formDataAttributes.Any())
            {
                foreach (var formDataAttribute in formDataAttributes)
                {
                    var formDataParameterName = formDataAttribute.TryGetPropertyValue("Name", default(string));
                    var formDataParameterDescription = formDataAttribute.TryGetPropertyValue("Description", default(string));
                    var formDataParameterType = formDataAttribute.TryGetPropertyValue("Type", default(Type));
                    var formDataParameterRequired = formDataAttribute.TryGetPropertyValue("Required", false);
                    if (formDataParameterType != null && formDataParameterName != null)
                        parameters.Add(new SwaggerMethodAttributeParameter(formDataParameterName, SwaggerMethodAttributeParameterType.Form,
                            formDataParameterType, formDataParameterRequired, formDataParameterDescription));
                }
            }

            foreach (var parameterObj in parameters)
            {
                bool isRegularParameter = parameterObj is ParameterInfo;
                var parameterName = isRegularParameter
                    ? ((ParameterInfo)parameterObj).Name
                    : ((SwaggerMethodAttributeParameter)parameterObj).Name;
                
                var attributes = isRegularParameter
                    ? ((ParameterInfo) parameterObj).GetCustomAttributes().ToList()
                    : new List<Attribute>();

                var bodyParameterName = parameterName;
                
                // All parameters in Azure Function signature are URI parameters; the definition is strict, only
                // binding parameters, URI parameters and Functions specific parameters (HttpRequest, TraceWriter, etc.)
                // are allowed as Azure Function parameters.

                // Exception to the above: AzureFunctionsV2.HttpExtensions package defines the HttpParam<T> class,
                // decorated with HttpBody, HttpQuery, HttpForm, HttpHeader attributes. These parameters can be
                // interpreted.

                var uriParameterName = parameterName;
                var uriParameterNameLower = uriParameterName.ToLowerInvariant();
                SwaggerParameter operationParameter = null;

                var lowerHttpPath = httpPath.ToLowerInvariant();
                if (lowerHttpPath.Contains("{" + uriParameterNameLower + "}") ||
                    lowerHttpPath.Contains("{" + uriParameterNameLower + ":")) // path parameter
                {
                    operationParameter = await context.SwaggerGenerator.CreatePrimitiveParameterAsync(
                        uriParameterName, (parameterObj as ParameterInfo)).ConfigureAwait(false);
                    operationParameter.Kind = SwaggerParameterKind.Path;
                    operationParameter.IsRequired = true; // Path is always required => property not needed

                    if (_settings.SchemaType == SchemaType.Swagger2)
                        operationParameter.IsNullableRaw = false;

                    context.OperationDescription.Operation.Parameters.Add(operationParameter);
                }
                else
                {
                    if (!isRegularParameter && ((SwaggerMethodAttributeParameter)parameterObj).ParameterType == 
                        SwaggerMethodAttributeParameterType.File)
                    {
                        var fileParameter = (SwaggerMethodAttributeParameter) parameterObj;
                        var synthesizedAttributes = new List<Attribute>();
                        if(((SwaggerMethodAttributeParameter)parameterObj).Required)
                            attributes.Add(new RequiredAttribute());
                        operationParameter = await AddFileParameterAsync(context, fileParameter.Name, fileParameter.Description,
                            (bool) fileParameter.Properties["MultiFile"], synthesizedAttributes);
                        context.OperationDescription.Operation.Parameters.Add(operationParameter);
                    }
                    else
                    {
                        Type parameterType = isRegularParameter
                            ? ((ParameterInfo)parameterObj).ParameterType
                            : ((SwaggerMethodAttributeParameter)parameterObj).ExplicitType;

                        var parameterInfo = _settings.ReflectionService
                            .GetDescription(parameterType, attributes, _settings);

                        if (isRegularParameter)
                        {
                            // AzureFunctionsV2.HttpExtensions support right here.
                            if (parameterType.Name == "HttpParam`1")
                            {
                                var parameterContainerValueType = parameterType.GetGenericArguments().FirstOrDefault();
                                var paramAttributes = ((ParameterInfo) parameterObj).GetCustomAttributes().ToList();
                                var paramHttpExtensionAttribute = paramAttributes.FirstOrDefault(x =>
                                    x.GetType().InheritsFrom("HttpSourceAttribute", TypeNameStyle.Name));

                                // Interpret the type and its attributes into attributes the SwaggerGenerator can understand.
                                var synthesizedAttributes = new List<Attribute>();
                                var required = paramHttpExtensionAttribute.TryGetPropertyValue<bool>("Required");
                                if(required)
                                    synthesizedAttributes.Add(new RequiredAttribute());
                                var annotatedParamName =
                                    paramHttpExtensionAttribute.TryGetPropertyValue<string>("Name");
                                if (!string.IsNullOrEmpty(annotatedParamName))
                                    parameterName = annotatedParamName;
                                
                                operationParameter = await context.SwaggerGenerator.CreatePrimitiveParameterAsync(
                                    parameterName, /* todo */ "",
                                    parameterContainerValueType, synthesizedAttributes);

                                if (paramHttpExtensionAttribute.GetType().Name == "HttpFormAttribute")
                                {
                                    operationParameter.Kind = SwaggerParameterKind.FormData;
                                    if (parameterContainerValueType.Name == "IFormFile" ||
                                        parameterContainerValueType.Name == "IFormFileCollection")
                                    {
                                        operationParameter = await AddFileParameterAsync(context, parameterName,
                                            "" /* todo */,
                                            parameterContainerValueType.Name == "IFormFileCollection", 
                                            synthesizedAttributes);
                                    }
                                }
                                else if (paramHttpExtensionAttribute.GetType().Name == "HttpQueryAttribute")
                                    operationParameter.Kind = SwaggerParameterKind.Query;
                                else if (paramHttpExtensionAttribute.GetType().Name == "HttpHeaderAttribute")
                                    operationParameter.Kind = SwaggerParameterKind.Header;
                                else if (paramHttpExtensionAttribute.GetType().Name == "HttpBodyAttribute")
                                {
                                    operationParameter = await AddSwaggerRequestBodyTypeParameterAsync(context, parameterName,
                                        parameterContainerValueType,
                                        synthesizedAttributes, required, "" /* todo */);
                                }

                                context.OperationDescription.Operation.Parameters.Add(operationParameter);

                            }

                        }
                        else
                        {
                            var swaggerMethodAttributeParameter = (SwaggerMethodAttributeParameter) parameterObj;
                            if (swaggerMethodAttributeParameter.ParameterType == SwaggerMethodAttributeParameterType.TypedBody)
                            {
                                operationParameter = await AddSwaggerRequestBodyTypeParameterAsync(context,
                                    swaggerMethodAttributeParameter.Name,
                                    swaggerMethodAttributeParameter.ExplicitType, new List<Attribute>(),
                                    swaggerMethodAttributeParameter.Required,
                                    swaggerMethodAttributeParameter.Description);

                                context.OperationDescription.Operation.Parameters.Add(operationParameter);
                            }
                            else if (swaggerMethodAttributeParameter.ParameterType == SwaggerMethodAttributeParameterType.Form)
                            {
                                operationParameter = await context.SwaggerGenerator.CreatePrimitiveParameterAsync(
                                    swaggerMethodAttributeParameter.Name, swaggerMethodAttributeParameter.Description,
                                    swaggerMethodAttributeParameter.ExplicitType, new Attribute[] { }).ConfigureAwait(false);
                                operationParameter.Kind = SwaggerParameterKind.FormData;
                                operationParameter.IsRequired = swaggerMethodAttributeParameter.Required;

                                context.OperationDescription.Operation.Parameters.Add(operationParameter);
                            }
                            else if (swaggerMethodAttributeParameter.ParameterType == SwaggerMethodAttributeParameterType.Header)
                            {
                                operationParameter = await context.SwaggerGenerator.CreatePrimitiveParameterAsync(
                                    swaggerMethodAttributeParameter.Name, swaggerMethodAttributeParameter.Description,
                                    swaggerMethodAttributeParameter.ExplicitType, new Attribute[] { }).ConfigureAwait(false);
                                operationParameter.Kind = SwaggerParameterKind.Header;
                                operationParameter.IsRequired = swaggerMethodAttributeParameter.Required;

                                context.OperationDescription.Operation.Parameters.Add(operationParameter);
                            }
                            else if (swaggerMethodAttributeParameter.ParameterType == SwaggerMethodAttributeParameterType.Query)
                            {
                                operationParameter = await context.SwaggerGenerator.CreatePrimitiveParameterAsync(
                                    swaggerMethodAttributeParameter.Name, swaggerMethodAttributeParameter.Description,
                                    swaggerMethodAttributeParameter.ExplicitType, new Attribute[] { }).ConfigureAwait(false);
                                operationParameter.Kind = SwaggerParameterKind.Query;
                                operationParameter.IsRequired = swaggerMethodAttributeParameter.Required;
                                // operationParameter.IsRequired = operationParameter.IsRequired || parameter.HasDefaultValue == false;

                                // if (parameter.HasDefaultValue)
                                //    operationParameter.Default = parameter.DefaultValue;

                                context.OperationDescription.Operation.Parameters.Add(operationParameter);
                            }
                        }
                        
                    }
                    
                }

                if (operationParameter != null)
                {
                    operationParameter.Position = position;
                    position++;

                    if (_settings.SchemaType == SchemaType.OpenApi3)
                    {
                        operationParameter.IsNullableRaw = null;
                    }
                }
                
            }

            if (_settings.AddMissingPathParameters)
            {
                foreach (Match match in Regex.Matches(httpPath, "{(.*?)(:(([^/]*)?))?}"))
                {
                    var parameterName = match.Groups[1].Value;
                    if (context.OperationDescription.Operation.Parameters.All(p => !string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var parameterType = match.Groups.Count == 5 ? match.Groups[3].Value : "string";
                        var operationParameter = context.SwaggerGenerator.CreateUntypedPathParameter(parameterName, parameterType);
                        context.OperationDescription.Operation.Parameters.Add(operationParameter);
                    }
                }
            }

            RemoveUnusedPathParameters(context.OperationDescription, httpPath);
            UpdateConsumedTypes(context.OperationDescription);

            return true;
        }

        private void UpdateConsumedTypes(SwaggerOperationDescription operationDescription)
        {
            if (operationDescription.Operation.ActualParameters.Any(p => p.Type == JsonObjectType.File))
                operationDescription.Operation.Consumes = new List<string> { "multipart/form-data" };
        }

        private void RemoveUnusedPathParameters(SwaggerOperationDescription operationDescription, string httpPath)
        {
            operationDescription.Path = Regex.Replace(httpPath, "{(.*?)(:(([^/]*)?))?}", match =>
            {
                var parameterName = match.Groups[1].Value.TrimEnd('?');
                if (operationDescription.Operation.ActualParameters.Any(p => p.Kind == SwaggerParameterKind.Path && string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase)))
                    return "{" + parameterName + "}";
                return string.Empty;
            }).TrimEnd('/');
        }

        private async Task<SwaggerParameter> AddSwaggerRequestBodyTypeParameterAsync(OperationProcessorContext context,
            string parameterName, Type parameterType, List<Attribute> attributes, bool required,
            string description)
        {
            SwaggerParameter operationParameter;

            var typeDescription = _settings.ReflectionService.GetDescription(
                parameterType,
                attributes, _settings);
            var isNullable = _settings.AllowNullableBodyParameters && typeDescription.IsNullable;

            var operation = context.OperationDescription.Operation;
            if (parameterType.Name == "XmlDocument" ||
                parameterType.InheritsFrom("XmlDocument", TypeNameStyle.Name))
            {
                operation.Consumes = new List<string> { "application/xml" };
                operationParameter = new SwaggerParameter
                {
                    Name = parameterName ?? "Body",
                    Kind = SwaggerParameterKind.Body,
                    Schema = new JsonSchema4
                    {
                        Type = JsonObjectType.String,
                        IsNullableRaw = isNullable
                    },
                    IsNullableRaw = isNullable,
                    IsRequired = required,
                    Description = description
                };
                // operation.Parameters.Add(operationParameter);
            }
            // TODO: need to check if this makes any sense. Need a test case.
            else if (parameterType.IsAssignableTo("System.IO.Stream", TypeNameStyle.FullName))
            {
                operation.Consumes = new List<string> { "application/octet-stream" };
                operationParameter = new SwaggerParameter
                {
                    Name = parameterName ?? "Body",
                    Kind = SwaggerParameterKind.Body,
                    Schema = new JsonSchema4
                    {
                        Type = JsonObjectType.String,
                        Format = JsonFormatStrings.Byte,
                        IsNullableRaw = isNullable
                    },
                    IsNullableRaw = isNullable,
                    IsRequired = required,
                    Description = description
                };
                // operation.Parameters.Add(operationParameter);
            }
            else
            {
                operationParameter = new SwaggerParameter
                {
                    Name = parameterName ?? "Body",
                    Kind = SwaggerParameterKind.Body,
                    IsRequired = true, // FromBody parameters are always required
                    IsNullableRaw = isNullable,
                    Description = description,
                    Schema = await context.SchemaGenerator.GenerateWithReferenceAndNullabilityAsync<JsonSchema4>(
                        parameterType, attributes, isNullable,
                        schemaResolver: context.SchemaResolver).ConfigureAwait(false)
                };
                // operation.Parameters.Add(operationParameter);
            }

            return operationParameter;
        }

        private async Task<SwaggerParameter> AddSwaggerRequestBodyTypeParameterAsync(OperationProcessorContext context, 
            SwaggerMethodAttributeParameter swaggerMethodAttributeParameter)
        {
            SwaggerParameter operationParameter;

            var typeDescription = _settings.ReflectionService.GetDescription(
                swaggerMethodAttributeParameter.ExplicitType,
                new Attribute[] { }, _settings);
            var isNullable = _settings.AllowNullableBodyParameters && typeDescription.IsNullable;

            var operation = context.OperationDescription.Operation;
            if (swaggerMethodAttributeParameter.ExplicitType.Name == "XmlDocument" || 
                swaggerMethodAttributeParameter.ExplicitType.InheritsFrom("XmlDocument", TypeNameStyle.Name))
            {
                operation.Consumes = new List<string> { "application/xml" };
                operationParameter = new SwaggerParameter
                {
                    Name = swaggerMethodAttributeParameter.Name ?? "Body",
                    Kind = SwaggerParameterKind.Body,
                    Schema = new JsonSchema4
                    {
                        Type = JsonObjectType.String,
                        IsNullableRaw = isNullable
                    },
                    IsNullableRaw = isNullable,
                    IsRequired = swaggerMethodAttributeParameter.Required,
                    Description = swaggerMethodAttributeParameter.Description
                };
                operation.Parameters.Add(operationParameter);
            }
            // TODO: need to check if this makes any sense. Need a test case.
            else if (swaggerMethodAttributeParameter.ExplicitType.IsAssignableTo("System.IO.Stream", TypeNameStyle.FullName))
            {
                operation.Consumes = new List<string> { "application/octet-stream" };
                operationParameter = new SwaggerParameter
                {
                    Name = swaggerMethodAttributeParameter.Name ?? "Body",
                    Kind = SwaggerParameterKind.Body,
                    Schema = new JsonSchema4
                    {
                        Type = JsonObjectType.String,
                        Format = JsonFormatStrings.Byte,
                        IsNullableRaw = isNullable
                    },
                    IsNullableRaw = isNullable,
                    IsRequired = swaggerMethodAttributeParameter.Required,
                    Description = swaggerMethodAttributeParameter.Description
                };
                operation.Parameters.Add(operationParameter);
            }
            else
            {
                operationParameter = new SwaggerParameter
                {
                    Name = swaggerMethodAttributeParameter.Name ?? "Body",
                    Kind = SwaggerParameterKind.Body,
                    IsRequired = true, // FromBody parameters are always required
                    IsNullableRaw = isNullable,
                    Description = swaggerMethodAttributeParameter.Description,
                    Schema = await context.SchemaGenerator.GenerateWithReferenceAndNullabilityAsync<JsonSchema4>(
                        swaggerMethodAttributeParameter.ExplicitType, new Attribute[] { }, isNullable, 
                        schemaResolver: context.SchemaResolver).ConfigureAwait(false)
                };
                operation.Parameters.Add(operationParameter);
            }

            return operationParameter;
        }

        private async Task<SwaggerParameter> AddFileParameterAsync(
            OperationProcessorContext context, string name, string description, bool multiFile, IEnumerable<Attribute> attributes)
        {
            var parameterDocumentation = description ?? "";
            var operationParameter = await context.SwaggerGenerator.CreatePrimitiveParameterAsync(
                name, description, typeof(IFormFile), attributes).ConfigureAwait(false);

            InitializeFileParameter(operationParameter, multiFile);
            
            return operationParameter;
        }

        private void InitializeFileParameter(SwaggerParameter operationParameter, bool isFileArray)
        {
            operationParameter.Type = JsonObjectType.File;
            operationParameter.Kind = SwaggerParameterKind.FormData;

            if (isFileArray)
                operationParameter.CollectionFormat = SwaggerParameterCollectionFormat.Multi;
        }
    }
}