using System;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NJsonSchema.Infrastructure;
using NSwag.Annotations.AzureFunctionsV2;

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
        /// <param name="req"></param>
        /// <param name="simpleQueryParam">Simple query param</param>
        /// <param name="secondQueryParam">Blah</param>
        /// <param name="objectQueryParam"></param>
        /// <param name="header"></param>
        /// <param name="log"></param>
        /// <returns></returns>
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

        [SwaggerAuthorize()]
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

        [FunctionName("swagger")]
        public static async Task<IActionResult> Swagger(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var generator = new AzureFunctionsV2ToSwaggerGenerator(new AzureFunctionsV2ToSwaggerGeneratorSettings());
            var document = await generator.GenerateForAzureFunctionClassAsync(typeof(Function1));
            var json = document.ToJson();
            return new OkObjectResult(json);
        }
    }
}
