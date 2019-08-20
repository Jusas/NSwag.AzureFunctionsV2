using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Infrastructure;
using NSwag.Generation;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2
{
    /// <summary>
    /// The main SwaggerGenerator that produces the Swagger document out of Azure Function assemblies.
    /// </summary>
    public class AzureFunctionsV2ToSwaggerGenerator
    {
        // private readonly OpenApiSchemaGenerator _schemaGenerator;

        /// <summary>
        /// Swagger generator settings.
        /// </summary>
        public AzureFunctionsV2ToSwaggerGeneratorSettings Settings { get; }

        /// <summary>Initializes a new instance of the <see cref="AzureFunctionsV2ToSwaggerGenerator" /> class.</summary>
        /// <param name="settings">The settings</param>
        public AzureFunctionsV2ToSwaggerGenerator(AzureFunctionsV2ToSwaggerGeneratorSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Returns Azure Function classes from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan for static classes with Functions</param>
        /// <returns>The Azure Function class types</returns>
        public static IEnumerable<Type> GetAzureFunctionClasses(Assembly assembly)
        {
            // Get classes that are: 1) static, 2) have static methods that have the FunctionName attribute, 3) are not ignored.
            
            var staticAzureFunctionClasses = assembly.ExportedTypes
                .Where(x => x.IsClass)
                .Where(x => x.GetCustomAttributes().All(a => a.GetType().Name != "SwaggerIgnoreAttribute" && a.GetType().Name != "OpenApiIgnoreAttribute"))
                .Where(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).Any(m => m.GetCustomAttributes().SingleOrDefault(a => a.GetType().Name == "FunctionNameAttribute") != null));

            return staticAzureFunctionClasses;
        }

        /// <summary>
        /// Generates Swagger document for the specified Azure Function classes, including only the listed Functions.
        /// </summary>
        /// <param name="azureFunctionClassTypes">The Azure Function classes (static classes)</param>
        /// <param name="functionNames">The function names (defined by FunctionNameAttribute)</param>
        /// <returns>The generated Swagger document</returns>
        public async Task<OpenApiDocument> GenerateForAzureFunctionClassesAsync(IEnumerable<Type> azureFunctionClassTypes, 
            IList<string> functionNames)
        {
            var document = await CreateDocumentAsync().ConfigureAwait(false);
            var schemaResolver = new OpenApiSchemaResolver(document, Settings);
            var usedAzureFunctionClassTypes = new List<Type>();

            foreach (var azureFunctionClassType in azureFunctionClassTypes)
            {
                var generator = new OpenApiDocumentGenerator(Settings, schemaResolver);
                var isIncluded = await GenerateForAzureFunctionClassAsync(document, azureFunctionClassType, 
                    generator, schemaResolver, functionNames);
                if(isIncluded)
                    usedAzureFunctionClassTypes.Add(azureFunctionClassType);
            }

            document.GenerateOperationIds();

            foreach (var processor in Settings.DocumentProcessors)
            {
                processor.Process(new DocumentProcessorContext(document, azureFunctionClassTypes,
                    usedAzureFunctionClassTypes, schemaResolver, Settings.SchemaGenerator, Settings));
            }

            return document;
        }

        /// <summary>
        /// Generates Swagger document for the specified Azure Function class type.
        /// </summary>
        /// <typeparam name="TAzureFunctionClass">The <see cref="Type"/> of the class</typeparam>
        /// <returns>The generated Swagger document</returns>
        public Task<OpenApiDocument> GenerateForAzureFunctionClassAsync<TAzureFunctionClass>()
        {
            return GenerateForAzureFunctionClassesAsync(new[] { typeof(TAzureFunctionClass) }, null);
        }

        /// <summary>
        /// Generates Swagger document for the specified Azure Function class type.
        /// </summary>
        /// <param name="azureFunctionClassType">The <see cref="Type"/> of the class</param>
        /// <returns>The generated Swagger document</returns>
        public Task<OpenApiDocument> GenerateForAzureFunctionClassAsync(Type azureFunctionClassType)
        {
            return GenerateForAzureFunctionClassesAsync(new[] { azureFunctionClassType }, null);
        }

        /// <summary>
        /// Generates Swagger document for the specified Azure Function class type, including only the listed Functions.
        /// </summary>
        /// <param name="azureFunctionClassType">The <see cref="Type"/> of the class</param>
        /// <param name="functionNames">The function names (defined by FunctionNameAttribute)</param>
        /// <returns>The generated Swagger document</returns>
        public Task<OpenApiDocument> GenerateForAzureFunctionClassAndSpecificMethodsAsync(Type azureFunctionClassType,
            IList<string> functionNames)
        {
            return GenerateForAzureFunctionClassesAsync(new[] { azureFunctionClassType }, functionNames);
        }


        private async Task<bool> GenerateForAzureFunctionClassAsync(OpenApiDocument document, Type staticAzureFunctionClassType,
            OpenApiDocumentGenerator swaggerGenerator, OpenApiSchemaResolver schemaResolver, IList<string> functionNames)
        {
            var operations = new List<Tuple<OpenApiOperationDescription, MethodInfo>>();

            foreach (var method in GetActionMethods(staticAzureFunctionClassType, functionNames))
            {
                var httpPaths = GetHttpPaths(method);
                var httpMethods = GetSupportedHttpMethods(method);

                foreach (var httpPath in httpPaths)
                {
                    foreach (var httpMethod in httpMethods)
                    {
                        var operationDescription = new OpenApiOperationDescription
                        {
                            Path = httpPath,
                            Method = httpMethod,
                            Operation = new OpenApiOperation
                            {
                                IsDeprecated = method.GetCustomAttribute<ObsoleteAttribute>() != null,
                                OperationId = GetOperationId(document, staticAzureFunctionClassType.Name, method)
                            }
                        };

                        operations.Add(new Tuple<OpenApiOperationDescription, MethodInfo>(operationDescription, method));
                    }
                }
            }

            return await AddOperationDescriptionsToDocumentAsync(document, staticAzureFunctionClassType, operations,
                swaggerGenerator, schemaResolver);
        }

        private async Task<bool> AddOperationDescriptionsToDocumentAsync(OpenApiDocument document, Type staticAzureFunctionClassType,
            List<Tuple<OpenApiOperationDescription, MethodInfo>> operations, OpenApiDocumentGenerator swaggerGenerator,
            OpenApiSchemaResolver schemaResolver)
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
                        document.Paths[path] = new OpenApiPathItem();

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

        // TODO: remove asyncness as most NSwag operations have been converted to sync.
        private async Task<bool> RunOperationProcessorsAsync(OpenApiDocument document, Type staticAzureFunctionClassType, MethodInfo methodInfo, 
            OpenApiOperationDescription operationDescription, List<OpenApiOperationDescription> allOperations, OpenApiDocumentGenerator swaggerGenerator, 
            OpenApiSchemaResolver schemaResolver)
        {
            var context = new OperationProcessorContext(document, operationDescription, staticAzureFunctionClassType,
                methodInfo, swaggerGenerator, Settings.SchemaGenerator, schemaResolver, Settings, allOperations);

            // 1. Run from settings
            foreach (var operationProcessor in Settings.OperationProcessors)
            {
                if (operationProcessor.Process(context) == false)
                    return false;
            }

            
            // 2. Run from class attributes
            var operationProcessorAttribute = methodInfo.DeclaringType.GetTypeInfo()
                .GetCustomAttributes()
                // 3. Run from method attributes
                .Concat(methodInfo.GetCustomAttributes())
                .Where(a => a.GetType().IsAssignableToTypeName("SwaggerOperationProcessorAttribute", TypeNameStyle.Name));

            foreach (dynamic attribute in operationProcessorAttribute)
            {
                var operationProcessor = ObjectExtensions.HasProperty(attribute, "Parameters") ?
                    (IOperationProcessor)Activator.CreateInstance(attribute.Type, attribute.Parameters) :
                    (IOperationProcessor)Activator.CreateInstance(attribute.Type);

                if (operationProcessor.Process(context) == false)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Get methods from a class type which are Azure Functions.
        /// </summary>
        /// <param name="azureFunctionStaticClassType">The Function App static class type</param>
        /// <param name="functionNames">The list of Function names to include. If null, will include all Functions,
        /// unless ignored via ignore attributes.</param>
        /// <returns></returns>
        private static IEnumerable<MethodInfo> GetActionMethods(Type azureFunctionStaticClassType, IList<string> functionNames)
        {
            var methods = azureFunctionStaticClassType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
            methods = methods.Where(x => x.GetCustomAttributes().Any(a => a.GetType().Name == "FunctionNameAttribute")).ToArray();
            methods = methods.Where(x => x.GetCustomAttributes().All(a => a.GetType().Name != "SwaggerIgnoreAttribute" &&
                                                                          a.GetType().Name != "OpenApiIgnoreAttribute" &&
                                                                          a.GetType().Name != "NonActionAttribute")).ToArray();
            methods = methods.Where(x => x.GetParameters().Any(p => p.ParameterType.Name == "HttpRequest")).ToArray();
            if (functionNames != null)
            {
                methods = methods.Where(x =>
                        functionNames.Any(f => f.Equals(
                            x.GetCustomAttributes().First(a => a.GetType().Name == "FunctionNameAttribute")
                                .TryGetPropertyValue<string>("Name"), StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
            }

            return methods;
        }

        /// <summary>
        /// Get the HTTP paths of an Azure Function.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private IEnumerable<string> GetHttpPaths(MethodInfo method)
        {
            // Default route is /api/<FunctionName>
            var defaultMethodName = method.GetCustomAttributes()
                .Single(x => x.GetType().Name == "FunctionNameAttribute")
                .TryGetPropertyValue("Name", default(string));
            var routeTemplate = defaultMethodName;
            
            // If route is defined in the HttpTrigger, we grab that.
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
                .Select(x => "/api/" + x
                                 .Replace("[", "{")
                                 .Replace("]", "}")
                                 .Replace("{*", "{")
                                 .Trim('/')                                 
                             ).Distinct().ToList();

            return httpPaths;
        }

        /// <summary>
        /// Get the supported HTTP methods of an Azure Function.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSupportedHttpMethods(MethodInfo method)
        {
            // Grab the methods from the HttpTrigger.
            var httpRequestParameter = method.GetParameters()
                .SingleOrDefault(x => x.ParameterType.Name == "HttpRequest");
            var httpTriggerAttribute = httpRequestParameter?.GetCustomAttributes()
                .SingleOrDefault(x => x.GetType().Name == "HttpTriggerAttribute");
            var methodsPropertyValue = httpTriggerAttribute.TryGetPropertyValue("Methods", default(string[]));
            List<string> httpMethods = new List<string>();

            if(methodsPropertyValue != null && methodsPropertyValue.Any())
            {
                foreach (var httpMethod in methodsPropertyValue)
                {
                    if(httpMethod.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(OpenApiOperationMethod.Get);
                    else if (httpMethod.StartsWith("post", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(OpenApiOperationMethod.Post);
                    else if (httpMethod.StartsWith("put", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(OpenApiOperationMethod.Put);
                    else if (httpMethod.StartsWith("patch", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(OpenApiOperationMethod.Patch);
                    else if (httpMethod.StartsWith("delete", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(OpenApiOperationMethod.Delete);
                    else if (httpMethod.StartsWith("head", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(OpenApiOperationMethod.Head);
                    else if (httpMethod.StartsWith("options", StringComparison.OrdinalIgnoreCase))
                        httpMethods.Add(OpenApiOperationMethod.Options);
                }
            }

            if(!httpMethods.Any())
                httpMethods.Add(OpenApiOperationMethod.Get);

            return httpMethods;
        }

        private string GetOperationId(OpenApiDocument document, string staticAzureFunctionClassName, MethodInfo method)
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

        /// <summary>
        /// Create a Swagger document with settings applied.
        /// </summary>
        /// <returns></returns>
        private async Task<OpenApiDocument> CreateDocumentAsync()
        {
            var document = !string.IsNullOrEmpty(Settings.DocumentTemplate) ?
                await OpenApiDocument.FromJsonAsync(Settings.DocumentTemplate).ConfigureAwait(false) :
                new OpenApiDocument();

            document.Generator = "NSwag v" + OpenApiDocument.ToolchainVersion + " (NJsonSchema v" + JsonSchema.ToolchainVersion + ")";
            document.SchemaType = Settings.SchemaType;

            document.Consumes = new List<string> { "application/json" };
            document.Produces = new List<string> { "application/json" };

            if (document.Info == null)
                document.Info = new OpenApiInfo();

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