using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NJsonSchema.Infrastructure;
using NSwag.Annotations;
using NSwag.Annotations.AzureFunctionsV2;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Processors;
using NSwag.SwaggerGeneration.Processors.Security;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp
{
    public static class Function1
    {
        public class MyObject
        {
            public string Name { get; set; }
        }

        // Note: need to generate and copy the XML doc to the bin folder (copy to output dir puts it to the wrong dir)

        /// <summary>
        /// This is a summary.
        /// </summary>
        /// <param name="req">The request</param>
        /// <param name="simpleQueryParam">Simple query param</param>
        /// <param name="secondQueryParam">Blah</param>
        /// <param name="objectQueryParam">Object param</param>
        /// <param name="header">A header</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [Produces(typeof(string))]
        [FunctionName("Function1")]
        public static async Task<IActionResult> Basics(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpQuery]HttpParam<string> simpleQueryParam,
            [HttpQuery(Required = true, Name = "foo")]HttpParam<string> secondQueryParam,
            [HttpQuery]HttpParam<MyObject> objectQueryParam,
            [HttpHeader(Name = "x-my-header")]HttpParam<string> header,
            ILogger log)
        {

            return new OkObjectResult("ok");
        }

        [SwaggerAuthorize]
        [FunctionName("Function2")]
        public static async Task<IActionResult> PostBody(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "my/url")] HttpRequest req,
            [HttpQuery]HttpParam<string> simpleQueryParam,
            [HttpBody]HttpParam<MyObject> body,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }

        [FunctionName("Function3")]
        public static async Task<IActionResult> PostFile(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "file")] HttpRequest req,
            [HttpForm]HttpParam<string> formField,
            [HttpForm]HttpParam<IFormFile> file,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }

        [Consumes("application/www-form-url-encoded")]
        [FunctionName("Function4")]
        public static async Task<IActionResult> PostFormObject(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "formobj")] HttpRequest req,
            [HttpForm]HttpParam<MyObject> formField,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }

        [SwaggerResponse(200, typeof(string), Description = "default ok")]
        [SwaggerResponse(400, typeof(string), Description = "bad request")]
        [FunctionName("swagger")]
        public static async Task<IActionResult> Swagger(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();

            settings.OperationProcessors.Add(new OperationSecurityProcessor("Bearer", SwaggerSecuritySchemeType.OpenIdConnect));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("Bearer", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.OAuth2,
                Flow = SwaggerOAuth2Flow.Implicit,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = "https://jusas-tests.eu.auth0.com/authorize",
                        Scopes = new Dictionary<string, string>() { {"openid", "openid"}, {"profile", "profile"}, {"name", "name"} },
                        TokenUrl = "https://jusas-tests.eu.auth0.com/oauth/token"
                    }
                },
                Description = "Token"
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("Basic", SwaggerSecuritySchemeType.Basic));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("Basic", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.Basic,
                Scheme = "Basic",
                Description = "Basic auth"
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("HApiKey", SwaggerSecuritySchemeType.ApiKey, SwaggerSecurityApiKeyLocation.Header));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("HApiKey", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.ApiKey,
                Name = "x-apikey",
                In = SwaggerSecurityApiKeyLocation.Header
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("QApiKey", SwaggerSecuritySchemeType.ApiKey, SwaggerSecurityApiKeyLocation.Query));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("QApiKey", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.ApiKey,
                Name = "apikey",
                In = SwaggerSecurityApiKeyLocation.Query
            }));

            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);
            var funcClasses = new[]
            {
                typeof(Function1), typeof(GenerationAnnotationTests), typeof(HttpExtensionTests),
                typeof(RouteParamTests)
            };
            var document = await generator.GenerateForAzureFunctionClassesAsync(funcClasses, null);
            
            var json = document.ToJson();
            return new OkObjectResult(json);
        }

        [SwaggerIgnore]
        [FunctionName("swaggerui")]
        public static async Task<IActionResult> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "swaggerui/{staticfile}")] HttpRequest req,
            string staticfile,
            ILogger log)
        {
            var root = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Function1)).Location);
            var file = $@"{root}\..\SwaggerUi-files\{staticfile}";
            file = Path.GetFullPath(file);
            if (File.Exists(file))
            {
                string contentType;
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(file, out contentType))
                {
                    contentType = "application/octet-stream";
                }

                byte[] buf = null;
                using (var stream = File.OpenRead(file))
                {
                    buf = new byte[stream.Length];
                    stream.Read(buf, 0, buf.Length);
                }
                return new FileContentResult(buf, contentType);
            }

            return new NotFoundResult();
        }
    }
}
