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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NJsonSchema.Infrastructure;
using NSwag.Annotations;
using NSwag.Annotations.AzureFunctionsV2;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Processors;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsApp.Startup;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp
{
    public static class SwaggerEndpoints
    {
        /// <summary>
        /// Generates Swagger JSON.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [OpenApiIgnore]
        [FunctionName("swagger")]
        public static async Task<IActionResult> Swagger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            var generator = new AzureFunctionsV2ToSwaggerGenerator(SwaggerConfiguration.SwaggerGeneratorSettings);
            var funcClasses = new[]
            {
                typeof(SwaggerEndpoints),
                typeof(GenerationAnnotationTests),
                typeof(HttpExtensionTests),
                typeof(RouteParamTests)
            };
            var document = await generator.GenerateForAzureFunctionClassesAsync(funcClasses, null);
            
            // Workaround for NSwag global security bug, see https://github.com/RicoSuter/NSwag/pull/2305
            document.Security.Clear();

            var json = document.ToJson();
            return new OkObjectResult(json);
        }
        
        /// <summary>
        /// Serves SwaggerUI files.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="staticfile"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [OpenApiIgnore]
        [FunctionName("swaggerui")]
        public static async Task<IActionResult> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swaggerui/{staticfile}")] HttpRequest req,
            string staticfile,
            ILogger log)
        {
            var asm = Assembly.GetAssembly(typeof(SwaggerEndpoints));
            var files = asm.GetManifestResourceNames().Select(x => x.Replace("NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp.SwaggerUi.", ""))
                .ToList();
            int index = -1;
            if ((index = files.IndexOf(staticfile)) != -1)
            {
                var fileExt = staticfile.Split('.').Last();
                var types = new Dictionary<string, string>()
                {
                    {"png", "image/png"},
                    {"html", "text/html"},
                    {"js", "application/javascript"},
                    {"css", "text/css"},
                    {"map", "application/json"}
                };
                var fileMime = types.ContainsKey(fileExt) ? types[fileExt] : "application/octet-stream";
                using (var stream = asm.GetManifestResourceStream(asm.GetManifestResourceNames()[index]))
                {
                    var buf = new byte[stream.Length];
                    await stream.ReadAsync(buf, 0, buf.Length);
                    return new FileContentResult(buf, fileMime);
                }
            }

            return new NotFoundResult();

        }
    }
}
