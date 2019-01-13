using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Infrastructure;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2
{
    // TODO: warn on unsupported swagger attributes in function parametersa.
    public class AzureFunctionsV2ToSwaggerGenerator
    {
        private readonly SwaggerJsonSchemaGenerator _schemaGenerator;
        public AzureFunctionsV2ToSwaggerGeneratorSettings Settings { get; }

        /// <summary>Initializes a new instance of the <see cref="AzureFunctionsV2ToSwaggerGenerator" /> class.</summary>
        /// <param name="settings">The settings.</param>
        public AzureFunctionsV2ToSwaggerGenerator(AzureFunctionsV2ToSwaggerGeneratorSettings settings)
            : this(settings, new SwaggerJsonSchemaGenerator(settings))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AzureFunctionsV2ToSwaggerGenerator" /> class.</summary>
        /// <param name="settings">The settings.</param>
        /// <param name="schemaGenerator">The schema generator.</param>
        public AzureFunctionsV2ToSwaggerGenerator(AzureFunctionsV2ToSwaggerGeneratorSettings settings, SwaggerJsonSchemaGenerator schemaGenerator)
        {
            Settings = settings;
            _schemaGenerator = schemaGenerator;
        }

        public static IEnumerable<Type> GetAzureFunctionClasses(Assembly assembly)
        {
            // Get classes that are: 1) static, 2) have static methods that have the FunctionName attribute, 3) are not ignored.
            
            var staticAzureFunctionClasses = assembly.ExportedTypes
                .Where(x => x.IsAbstract && x.IsSealed && x.IsClass)
                .Where(x => x.GetCustomAttributes().All(a => a.GetType().Name != "SwaggerIgnoreAttribute"))
                .Where(x => x.GetMethods(BindingFlags.Static).Any(m => m.GetCustomAttributes().SingleOrDefault(a => a.GetType().Name == "FunctionNameAttribute") != null));

            return staticAzureFunctionClasses;
        }

        public async Task<SwaggerDocument> GenerateForAzureFunctionClassesAsync(IEnumerable<Type> azureFunctionClassTypes)
        {
            var document = await CreateDocumentAsync().ConfigureAwait(false);
            var schemaResolver = new SwaggerSchemaResolver(document, Settings);
            var usedAzureFunctionClassTypes = new List<Type>();

            foreach (var azureFunctionClassType in azureFunctionClassTypes)
            {
                var generator = new SwaggerGenerator(_schemaGenerator, Settings, schemaResolver);
                var isIncluded = await GenerateForAzureFunctionClassAsync(document, azureFunctionClassType, 
                    generator, schemaResolver);
                if(isIncluded)
                    usedAzureFunctionClassTypes.Add(azureFunctionClassType);
            }

            document.GenerateOperationIds();

            foreach (var processor in Settings.DocumentProcessors)
            {
                await processor.ProcessAsync(new DocumentProcessorContext(document, azureFunctionClassTypes,
                    usedAzureFunctionClassTypes, schemaResolver, _schemaGenerator, Settings));
            }

            return document;
        }

        public Task<SwaggerDocument> GenerateForAzureFunctionClassAsync<TAzureFunctionClass>()
        {
            return GenerateForAzureFunctionClassesAsync(new[] { typeof(TAzureFunctionClass) });
        }

        public Task<SwaggerDocument> GenerateForAzureFunctionClassAsync(Type azureFunctionClassType)
        {
            return GenerateForAzureFunctionClassesAsync(new[] { azureFunctionClassType });
        }

        private async Task<bool> GenerateForAzureFunctionClassAsync(SwaggerDocument document, Type staticAzureFunctionClassType,
            SwaggerGenerator swaggerGenerator, SwaggerSchemaResolver schemaResolver)
        {
            var operations = new List<Tuple<SwaggerOperationDescription, MethodInfo>>();

            foreach (var method in GetActionMethods(staticAzureFunctionClassType))
            {
                var httpPaths = GetHttpPaths(method);
                var httpMethods = GetSupportedHttpMethods(method);

                foreach (var httpPath in httpPaths)
                {
                    foreach (var httpMethod in httpMethods)
                    {
                        var operationDescription = new SwaggerOperationDescription
                        {
                            Path = httpPath,
                            Method = httpMethod,
                            Operation = new SwaggerOperation
                            {
                                IsDeprecated = method.GetCustomAttribute<ObsoleteAttribute>() != null,
                                OperationId = GetOperationId(document, staticAzureFunctionClassType.Name, method)
                            }
                        };

                        operations.Add(new Tuple<SwaggerOperationDescription, MethodInfo>(operationDescription, method));
                    }
                }
            }

            return await AddOperationDescriptionsToDocumentAsync(document, staticAzureFunctionClassType, operations,
                swaggerGenerator, schemaResolver);
        }

        private async Task<bool> AddOperationDescriptionsToDocumentAsync(SwaggerDocument document, Type staticAzureFunctionClassType,
            List<Tuple<SwaggerOperationDescription, MethodInfo>> operations, SwaggerGenerator swaggerGenerator,
            SwaggerSchemaResolver schemaResolver)
        {
            var addedOperations = 0;
            var allOps = operations.Select(t => t.Item1).ToList();
            foreach (var o in operations)
            {
                var operation = o.Item1;
                var method = o.Item2;

                var addOperation = await RunOperationProcessorsAsync(document, staticAzureFunctionClassType, method,
                    operation, allOps, swaggerGenerator, schemaResolver);
                if (addOperation)
                {
                    var path = operation.Path.Replace("//", "/");

                    if (!document.Paths.ContainsKey(path))
                        document.Paths[path] = new SwaggerPathItem();

                    if (document.Paths[path].ContainsKey(operation.Method))
                    {
                        throw new InvalidOperationException("The method '" + operation.Method + "' on path '" + path + "' is registered multiple times");
                    }

                    document.Paths[path][operation.Method] = operation.Operation;
                    addedOperations++;
                }

            }

            return addedOperations > 0;
        }

        private async Task<bool> RunOperationProcessorsAsync(SwaggerDocument document, Type staticAzureFunctionClassType, MethodInfo methodInfo, 
            SwaggerOperationDescription operationDescription, List<SwaggerOperationDescription> allOperations, SwaggerGenerator swaggerGenerator, 
            SwaggerSchemaResolver schemaResolver)
        {
            var context = new OperationProcessorContext(document, operationDescription, staticAzureFunctionClassType,
                methodInfo, swaggerGenerator, _schemaGenerator, schemaResolver, Settings, allOperations);

            // 1. Run from settings
            foreach (var operationProcessor in Settings.OperationProcessors)
            {
                if (await operationProcessor.ProcessAsync(context).ConfigureAwait(false) == false)
                    return false;
            }

            // TODO: find out what exactly are we doing here.
            // 2. Run from class attributes
            var operationProcessorAttribute = methodInfo.DeclaringType.GetTypeInfo()
                .GetCustomAttributes()
                // 3. Run from method attributes
                .Concat(methodInfo.GetCustomAttributes())
                .Where(a => a.GetType().IsAssignableTo("SwaggerOperationProcessorAttribute", TypeNameStyle.Name));

            foreach (dynamic attribute in operationProcessorAttribute)
            {
                var operationProcessor = ReflectionExtensions.HasProperty(attribute, "Parameters") ?
                    (IOperationProcessor)Activator.CreateInstance(attribute.Type, attribute.Parameters) :
                    (IOperationProcessor)Activator.CreateInstance(attribute.Type);

                if (await operationProcessor.ProcessAsync(context).ConfigureAwait(false) == false)
                    return false;
            }

            return true;
        }


        private static IEnumerable<MethodInfo> GetActionMethods(Type azureFunctionStaticClassType)
        {
            var methods = azureFunctionStaticClassType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            methods = methods.Where(x => x.GetCustomAttributes().Any(a => a.GetType().Name == "FunctionNameAttribute")).ToArray();
            methods = methods.Where(x => x.GetCustomAttributes().All(a => a.GetType().Name != "SwaggerIgnoreAttribute" &&
                                                             a.GetType().Name != "NonActionAttribute")).ToArray();
            methods = methods.Where(x => x.GetParameters().Any(p => p.ParameterType.Name == "HttpRequest")).ToArray();

            return methods;
        }

        private IEnumerable<string> GetHttpPaths(MethodInfo method)
        {
            var defaultMethodName = method.GetCustomAttributes()
                .Single(x => x.GetType().Name == "FunctionNameAttribute")
                .TryGetPropertyValue("Name", default(string));
            var routeTemplate = defaultMethodName;
            
            var httpRequestParameter = method.GetParameters()
                .SingleOrDefault(x => x.ParameterType.Name == "HttpRequest");
            var httpTriggerAttribute = httpRequestParameter?.GetCustomAttributes()
                .SingleOrDefault(x => x.GetType().Name == "HttpTriggerAttribute");
            var routePropertyValue = httpTriggerAttribute.TryGetPropertyValue("Route", default(string));

            if (httpTriggerAttribute != null && !string.IsNullOrEmpty(routePropertyValue))
            {
                routeTemplate = routePropertyValue;
            }

            var httpPaths = ExpandOptionalHttpPathParameters(routeTemplate, method)
                .Select(x => "api/" + x
                                 .Replace("[", "{")
                                 .Replace("]", "}")
                                 .Replace("{*", "{")
                                 .Trim('/')                                 
                             ).Distinct().ToList();

            return httpPaths;
        }

        private IEnumerable<SwaggerOperationMethod> GetSupportedHttpMethods(MethodInfo method)
        {
            var httpRequestParameter = method.GetParameters()
                .SingleOrDefault(x => x.ParameterType.Name == "HttpRequest");
            var httpTriggerAttribute = httpRequestParameter?.GetCustomAttributes()
                .SingleOrDefault(x => x.GetType().Name == "HttpTriggerAttribute");
            var methodsPropertyValue = httpTriggerAttribute.TryGetPropertyValue("Methods", default(string[]));
            List<SwaggerOperationMethod> httpMethods = new List<SwaggerOperationMethod>();

            if(methodsPropertyValue != null && methodsPropertyValue.Any())
            {
                foreach (var httpMethod in methodsPropertyValue)
                {
                    if(httpMethod.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(SwaggerOperationMethod.Get);
                    else if (httpMethod.StartsWith("post", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(SwaggerOperationMethod.Post);
                    else if (httpMethod.StartsWith("put", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(SwaggerOperationMethod.Put);
                    else if (httpMethod.StartsWith("patch", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(SwaggerOperationMethod.Patch);
                    else if (httpMethod.StartsWith("delete", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(SwaggerOperationMethod.Delete);
                    else if (httpMethod.StartsWith("head", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(SwaggerOperationMethod.Head);
                    else if (httpMethod.StartsWith("options", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(SwaggerOperationMethod.Options);
                }
            }

            if(!httpMethods.Any())
                httpMethods.Add(SwaggerOperationMethod.Get);

            return httpMethods;
        }

        private string GetOperationId(SwaggerDocument document, string staticAzureFunctionClassName, MethodInfo method)
        {
            string operationId;

            dynamic swaggerOperationAttribute = method.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "SwaggerOperationAttribute");
            if (swaggerOperationAttribute != null && !string.IsNullOrEmpty(swaggerOperationAttribute.OperationId))
                operationId = swaggerOperationAttribute.OperationId;
            else
            {
                var azureFunctionName = method.GetCustomAttributes().Single(a => a.GetType().Name == "FunctionNameAttribute")
                    .TryGetPropertyValue("Name", method.Name);
                operationId = staticAzureFunctionClassName + "_" + azureFunctionName;
            }

            var number = 1;
            while (document.Operations.Any(o => o.Operation.OperationId == operationId + (number > 1 ? "_" + number : string.Empty)))
                number++;

            return operationId + (number > 1 ? number.ToString() : string.Empty);
        }

        private IEnumerable<string> ExpandOptionalHttpPathParameters(string path, MethodInfo method)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment.EndsWith("?}"))
                {
                    // Only expand if optional parameter is available in action method
                    if (method.GetParameters().Any(p => segment.StartsWith("{" + p.Name + ":") || segment.StartsWith("{" + p.Name + "?")))
                    {
                        foreach (var p in ExpandOptionalHttpPathParameters(string.Join("/", segments.Take(i).Concat(new[] { segment.Replace("?", "") }).Concat(segments.Skip(i + 1))), method))
                            yield return p;
                    }
                    else
                    {
                        foreach (var p in ExpandOptionalHttpPathParameters(string.Join("/", segments.Take(i).Concat(segments.Skip(i + 1))), method))
                            yield return p;
                    }

                    yield break;
                }
            }

            yield return path;
        }

        private async Task<SwaggerDocument> CreateDocumentAsync()
        {
            var document = !string.IsNullOrEmpty(Settings.DocumentTemplate) ?
                await SwaggerDocument.FromJsonAsync(Settings.DocumentTemplate).ConfigureAwait(false) :
                new SwaggerDocument();

            document.Generator = "NSwag v" + SwaggerDocument.ToolchainVersion + " (NJsonSchema v" + JsonSchema4.ToolchainVersion + ")";
            document.SchemaType = Settings.SchemaType;

            document.Consumes = new List<string> { "application/json" };
            document.Produces = new List<string> { "application/json" };

            if (document.Info == null)
                document.Info = new SwaggerInfo();

            if (string.IsNullOrEmpty(Settings.DocumentTemplate))
            {
                if (!string.IsNullOrEmpty(Settings.Title))
                    document.Info.Title = Settings.Title;
                if (!string.IsNullOrEmpty(Settings.Description))
                    document.Info.Description = Settings.Description;
                if (!string.IsNullOrEmpty(Settings.Version))
                    document.Info.Version = Settings.Version;
            }

            return document;
        }
    }
}