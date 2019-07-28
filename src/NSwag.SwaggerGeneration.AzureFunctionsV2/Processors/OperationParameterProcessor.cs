using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Infrastructure;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NSwag.SwaggerGeneration.AzureFunctionsV2.ProcessorUtils;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Processors
{
    /// <summary>
    /// The processor that processes the operation parameters into the swagger document.
    /// </summary>
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

        /// <summary>
        /// Checks if a method parameter has attributes that are Binding attributes.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool HasBindingAttribute(ParameterInfo parameter)
        {
            // Support for AzureFunctionsV2.HttpExtensions. These ones need to pass this check.
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

        public bool Process(OperationProcessorContext context)
        {
            var httpPath = context.OperationDescription.Path;
            var position = 1;

            var ignoredParameterTypeNames = new string[] {"HttpRequest", "TraceWriter", "TextWriter", "ILogger", "ClaimsPrincipal", "CancellationToken"};
            var ignoreAttributeTypeNames = new string[] {"SwaggerIgnoreAttribute", "OpenApiIgnoreAttribute", "FromServicesAttribute", "BindNeverAttribute"};

            // Ignore parameters that are directly Azure Functions related or explicitly marked as ignored.
            // The resulting parameters should include the HttpParam<T> parameters that come from
            // AzureFunctionsV2.HttpExtensions, if there were any.
            var parameters = new List<object>(
                context.MethodInfo.GetParameters()
                    .Where(x => !ignoredParameterTypeNames.Contains(x.ParameterType.Name) && 
                                x.GetCustomAttributes().All(a => !ignoreAttributeTypeNames.Contains(a.GetType().Name)) &&
                                !HasBindingAttribute(x))
                );

            var methodAzureFunctionsSwaggerAttributes =
                OperationParameterProcessorUtils.GetAttributes(context.MethodInfo);

            if (methodAzureFunctionsSwaggerAttributes.UploadFileAttributes.Any())
            {
                foreach (var uploadFileAttribute in methodAzureFunctionsSwaggerAttributes.UploadFileAttributes)
                {
                    var formDataFileParamName = uploadFileAttribute.TryGetPropertyValue("Name", default(string));
                    var formDataFileParamDescription = uploadFileAttribute.TryGetPropertyValue("Description", default(string));
                    var formDataFileMultiFile = uploadFileAttribute.TryGetPropertyValue("MultiFile", default(bool));
                    parameters.Add(new SwaggerMethodAttributeParameter(formDataFileParamName, SwaggerMethodAttributeParameterType.File,
                        null, true, formDataFileParamDescription, new Dictionary<string, object>() { { "MultiFile", formDataFileMultiFile } }));
                }
            }

            if (methodAzureFunctionsSwaggerAttributes.RequestBodyTypeAttribute != null)
            {
                var bodyTypeAttribute = methodAzureFunctionsSwaggerAttributes.RequestBodyTypeAttribute;
                var requestBodyParamType = bodyTypeAttribute.TryGetPropertyValue("Type", default(Type));
                var requestBodyParamDescription = bodyTypeAttribute.TryGetPropertyValue("Description", default(string));
                var requestBodyParamName = bodyTypeAttribute.TryGetPropertyValue("Name", default(string));
                var requestBodyParamRequired = bodyTypeAttribute.TryGetPropertyValue("Required", false);
                if (string.IsNullOrEmpty(requestBodyParamName))
                    requestBodyParamName = "Body";
                parameters.Add(new SwaggerMethodAttributeParameter(requestBodyParamName, SwaggerMethodAttributeParameterType.TypedBody, 
                    requestBodyParamType, requestBodyParamRequired, requestBodyParamDescription));
            }

            if (methodAzureFunctionsSwaggerAttributes.HeaderAttributes.Any())
            {
                foreach (var headerAttribute in methodAzureFunctionsSwaggerAttributes.HeaderAttributes)
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

            if (methodAzureFunctionsSwaggerAttributes.QueryAttributes.Any())
            {
                foreach (var queryAttribute in methodAzureFunctionsSwaggerAttributes.QueryAttributes)
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

            if (methodAzureFunctionsSwaggerAttributes.FormDataAttributes.Any())
            {
                foreach (var formDataAttribute in methodAzureFunctionsSwaggerAttributes.FormDataAttributes)
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
                bool isRegularMethodParameter = parameterObj is ParameterInfo;
                var parameterName = isRegularMethodParameter
                    ? ((ParameterInfo)parameterObj).Name
                    : ((SwaggerMethodAttributeParameter)parameterObj).Name;
                
                var attributes = isRegularMethodParameter
                    ? ((ParameterInfo) parameterObj).GetCustomAttributes().ToList()
                    : new List<Attribute>();
                
                // All parameters in Azure Function signature are URI parameters; the definition is strict, only
                // binding parameters, URI parameters and Functions specific parameters (HttpRequest, TraceWriter, etc.)
                // are allowed as Azure Function parameters. Non-binding, non-route parameters are forbidden in Function signature.

                // Exception to the above: AzureFunctionsV2.HttpExtensions package defines the HttpParam<T> class,
                // decorated with HttpBody, HttpQuery, HttpForm, HttpHeader attributes. These parameters are binding parameters
                // from technical standpoint but can be interpreted as parameters holding request values.

                var uriParameterName = parameterName;
                var uriParameterNameLower = uriParameterName.ToLowerInvariant();
                OpenApiParameter operationParameter = null;

                var lowerHttpPath = httpPath.ToLowerInvariant();
                if (lowerHttpPath.Contains("{" + uriParameterNameLower + "}") ||
                    lowerHttpPath.Contains("{" + uriParameterNameLower + ":")) // path parameter
                {
                    operationParameter = context.DocumentGenerator.CreatePrimitiveParameter(
                        uriParameterName, (parameterObj as ParameterInfo).ToContextualParameter());
                    operationParameter.Kind = OpenApiParameterKind.Path;
                    operationParameter.IsRequired = true; // Path is always required => property not needed

                    if (_settings.SchemaType == SchemaType.Swagger2)
                        operationParameter.IsNullableRaw = false;

                    context.OperationDescription.Operation.Parameters.Add(operationParameter);
                }
                else
                {
                    // Non-regular parameter handling (ie. parameters that are not in function signature but as method attributes)
                    if (!isRegularMethodParameter)
                    {
                        var parameterAsAttribute = (SwaggerMethodAttributeParameter) parameterObj;

                        if (parameterAsAttribute.ParameterType == SwaggerMethodAttributeParameterType.File)
                        {
                            var synthesizedAttributes = new Attribute[] { };
                            if (((SwaggerMethodAttributeParameter)parameterObj).Required)
                                attributes.Add(new RequiredAttribute());
                            operationParameter = AddFileParameter(context, parameterAsAttribute.Name, parameterAsAttribute.Description,
                                (bool)parameterAsAttribute.Properties["MultiFile"], synthesizedAttributes);
                            context.OperationDescription.Operation.Parameters.Add(operationParameter);
                        }
                        else if (parameterAsAttribute.ParameterType == SwaggerMethodAttributeParameterType.TypedBody)
                        {
                            operationParameter = AddSwaggerRequestBodyTypeParameter(context,
                                parameterAsAttribute.Name,
                                parameterAsAttribute.ExplicitType, new List<Attribute>(),
                                parameterAsAttribute.Required,
                                parameterAsAttribute.Description);

                            context.OperationDescription.Operation.Parameters.Add(operationParameter);
                        }
                        else if (parameterAsAttribute.ParameterType == SwaggerMethodAttributeParameterType.Form)
                        {
                            operationParameter = context.DocumentGenerator.CreatePrimitiveParameter(
                                parameterAsAttribute.Name, parameterAsAttribute.Description,
                                parameterAsAttribute.ExplicitType.ToContextualType());
                            operationParameter.Kind = OpenApiParameterKind.FormData;
                            operationParameter.IsRequired = parameterAsAttribute.Required;

                            context.OperationDescription.Operation.Parameters.Add(operationParameter);
                        }
                        else if (parameterAsAttribute.ParameterType == SwaggerMethodAttributeParameterType.Header)
                        {
                            operationParameter = context.DocumentGenerator.CreatePrimitiveParameter(
                                parameterAsAttribute.Name, parameterAsAttribute.Description,
                                parameterAsAttribute.ExplicitType.ToContextualType());
                            operationParameter.Kind = OpenApiParameterKind.Header;
                            operationParameter.IsRequired = parameterAsAttribute.Required;

                            context.OperationDescription.Operation.Parameters.Add(operationParameter);
                        }
                        else if (parameterAsAttribute.ParameterType == SwaggerMethodAttributeParameterType.Query)
                        {
                            operationParameter = context.DocumentGenerator.CreatePrimitiveParameter(
                                parameterAsAttribute.Name, parameterAsAttribute.Description,
                                parameterAsAttribute.ExplicitType.ToContextualType());
                            operationParameter.Kind = OpenApiParameterKind.Query;
                            operationParameter.IsRequired = parameterAsAttribute.Required;

                            context.OperationDescription.Operation.Parameters.Add(operationParameter);
                        }
                    }
                    // Regular parameter handling. These ones should only be the HttpParam<T> type parameters
                    // coming from the usage of AzureFunctionsV2.HttpExtensions package.
                    else if(((ParameterInfo)parameterObj).ParameterType.Name == "HttpParam`1")
                    {
                        var httpParam = (ParameterInfo) parameterObj;                        
                        var httpParamDocumentation = httpParam.GetXmlDocs();
                        var httpParamType = httpParam.ParameterType;

                        var httpParamContainerValueType = httpParamType.GetGenericArguments().FirstOrDefault();
                        var paramAttributes = httpParam.GetCustomAttributes().ToList();
                        var paramHttpExtensionAttribute = paramAttributes.FirstOrDefault(x =>
                            x.GetType().InheritsFromTypeName("HttpSourceAttribute", TypeNameStyle.Name));

                        // Interpret the type and its attributes into attributes the SwaggerGenerator can understand.
                        var synthesizedAttributes = new List<Attribute>();
                        var required = paramHttpExtensionAttribute.TryGetPropertyValue<bool>("Required");
                        if (required)
                            synthesizedAttributes.Add(new RequiredAttribute());
                        var annotatedParamName =
                            paramHttpExtensionAttribute.TryGetPropertyValue<string>("Name");
                        if (!string.IsNullOrEmpty(annotatedParamName))
                            parameterName = annotatedParamName;

                        if(paramHttpExtensionAttribute.GetType().Name != "HttpBodyAttribute")
                            operationParameter = context.DocumentGenerator.CreatePrimitiveParameter(
                                parameterName, httpParamDocumentation,
                                httpParamContainerValueType.ToContextualType(synthesizedAttributes));

                        if (paramHttpExtensionAttribute.GetType().Name == "HttpFormAttribute")
                        {
                            operationParameter.Kind = OpenApiParameterKind.FormData;
                            if (httpParamContainerValueType.Name == "IFormFile" ||
                                httpParamContainerValueType.Name == "IFormFileCollection")
                            {
                                operationParameter = AddFileParameter(context, parameterName,
                                    httpParamDocumentation,
                                    httpParamContainerValueType.Name == "IFormFileCollection",
                                    synthesizedAttributes);
                            }
                        }
                        else if (paramHttpExtensionAttribute.GetType().Name == "HttpQueryAttribute")
                            operationParameter.Kind = OpenApiParameterKind.Query;
                        else if (paramHttpExtensionAttribute.GetType().Name == "HttpHeaderAttribute")
                            operationParameter.Kind = OpenApiParameterKind.Header;
                        else if (paramHttpExtensionAttribute.GetType().Name == "HttpBodyAttribute")
                        {
                            operationParameter = AddSwaggerRequestBodyTypeParameter(context, parameterName,
                                httpParamContainerValueType,
                                synthesizedAttributes, required, httpParamDocumentation /* todo */);
                        }

                        context.OperationDescription.Operation.Parameters.Add(operationParameter);
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
                        var operationParameter = context.DocumentGenerator.CreateUntypedPathParameter(parameterName, parameterType);
                        context.OperationDescription.Operation.Parameters.Add(operationParameter);
                    }
                }
            }

            RemoveUnusedPathParameters(context.OperationDescription, httpPath);
            UpdateConsumedTypes(context.OperationDescription);

            return true;
        }

        private void UpdateConsumedTypes(OpenApiOperationDescription operationDescription)
        {
            if (operationDescription.Operation.ActualParameters.Any(p => p.Type == JsonObjectType.File))
                operationDescription.Operation.Consumes = new List<string> { "multipart/form-data" };
            else if (operationDescription.Operation.ActualParameters.Any(p => p.Kind == OpenApiParameterKind.FormData))
                operationDescription.Operation.Consumes = new List<string>() {"application/x-www-form-urlencoded"};
        }

        private void RemoveUnusedPathParameters(OpenApiOperationDescription operationDescription, string httpPath)
        {
            operationDescription.Path = Regex.Replace(httpPath, "{(.*?)(:(([^/]*)?))?}", match =>
            {
                var parameterName = match.Groups[1].Value.TrimEnd('?');
                if (operationDescription.Operation.ActualParameters.Any(p => p.Kind == OpenApiParameterKind.Path && string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase)))
                    return "{" + parameterName + "}";
                return string.Empty;
            }).TrimEnd('/');
        }

        private OpenApiParameter AddSwaggerRequestBodyTypeParameter(OperationProcessorContext context,
            string parameterName, Type parameterType, List<Attribute> attributes, bool required,
            string description)
        {
            OpenApiParameter operationParameter;

            var typeDescription = _settings.ReflectionService.GetDescription(
                parameterType.ToContextualType(attributes),
                _settings);
            var isNullable = _settings.AllowNullableBodyParameters && typeDescription.IsNullable;

            var operation = context.OperationDescription.Operation;
            if (parameterType.Name == "XmlDocument" ||
                parameterType.InheritsFromTypeName("XmlDocument", TypeNameStyle.Name))
            {
                operation.Consumes = new List<string> { "application/xml" };
                operationParameter = new OpenApiParameter
                {
                    Name = parameterName ?? "Body",
                    Kind = OpenApiParameterKind.Body,
                    Schema = new JsonSchema
                    {
                        Type = JsonObjectType.String,
                        IsNullableRaw = isNullable
                    },
                    IsNullableRaw = isNullable,
                    IsRequired = required,
                    Description = description
                };
            }
            // TODO: need to check if this makes any sense. Need a test case.
            else if (parameterType.IsAssignableToTypeName("System.IO.Stream", TypeNameStyle.FullName))
            {
                operation.Consumes = new List<string> { "application/octet-stream" };
                operationParameter = new OpenApiParameter
                {
                    Name = parameterName ?? "Body",
                    Kind = OpenApiParameterKind.Body,
                    Schema = new JsonSchema
                    {
                        Type = JsonObjectType.String,
                        Format = JsonFormatStrings.Byte,
                        IsNullableRaw = isNullable
                    },
                    IsNullableRaw = isNullable,
                    IsRequired = required,
                    Description = description
                };
            }
            else
            {
                operationParameter = new OpenApiParameter
                {
                    Name = parameterName ?? "Body",
                    Kind = OpenApiParameterKind.Body,
                    IsRequired = required,
                    IsNullableRaw = isNullable,
                    Description = description,
                    Schema = context.SchemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(
                        parameterType.ToContextualType(attributes),
                        isNullable,
                        context.SchemaResolver)
            };
            }

            return operationParameter;
        }

        private OpenApiParameter AddFileParameter(
            OperationProcessorContext context, string name, string description, bool multiFile, IEnumerable<Attribute> attributes)
        {
            var parameterDocumentation = description ?? "";
            var operationParameter = context.DocumentGenerator.CreatePrimitiveParameter(
                name, parameterDocumentation, typeof(IFormFile).ToContextualType(attributes));

            InitializeFileParameter(operationParameter, multiFile);
            
            return operationParameter;
        }

        private void InitializeFileParameter(OpenApiParameter operationParameter, bool isFileArray)
        {
            operationParameter.Type = JsonObjectType.File;
            operationParameter.Kind = OpenApiParameterKind.FormData;

            if (isFileArray)
                operationParameter.CollectionFormat = OpenApiParameterCollectionFormat.Multi;
        }
    }
}