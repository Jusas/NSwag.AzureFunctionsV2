using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NSwag.Annotations.AzureFunctionsV2;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp.Helpers;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp
{
    public static class Functions1
    {
        [FunctionName("DefaultNoParamsNoAnnotations")]
        public static async Task<IActionResult> DefaultNoParamsNoAnnotations([HttpTrigger(
            AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Triggered");
            return new OkObjectResult("ok");
        }

        [FunctionName("PathParameters")]
        public static async Task<IActionResult> PathParameters([HttpTrigger(
            AuthorizationLevel.Function, "GET", Route = "test/{stringparam}/{intparam}")] HttpRequest req,
            string stringparam, int intparam,
            ILogger log)
        {
            log.LogInformation("Triggered");
            return new OkObjectResult($"stringparam: {stringparam}, intparam: {intparam}");
        }

        [SwaggerResponse(200, typeof(ResponseModelWithPrimitives), Description = "Description", IsNullable = false)]
        [FunctionName("ReturnValueAnnotation")]
        public static async Task<IActionResult> ReturnValueAnnotation([HttpTrigger(
            AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Triggered");
            return new OkObjectResult(new ResponseModelWithPrimitives() { IntValue = 1, StringValue = "hello swagger" });
        }

        [SwaggerRequestBodyType(typeof(string), true, "RequestPrimitive", "Description")]
        [FunctionName("PostPrimitiveTypeAnnotation")]
        public static async Task<IActionResult> PostPrimitiveTypeAnnotation([HttpTrigger(
                AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Triggered");
            var input = await req.ReadAsStringAsync();
            return new OkObjectResult(input);
        }

        [SwaggerRequestBodyType(typeof(RequestBodyModelWithPrimitives), true, "RequestModel", "Description")]
        [FunctionName("PostComplexTypeAnnotation")]
        public static async Task<IActionResult> PostComplexTypeAnnotation([HttpTrigger(
                AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Triggered");
            var input = JsonConvert.DeserializeObject<RequestBodyModelWithPrimitives>(await req.ReadAsStringAsync());            
            return new OkObjectResult(input);
        }

        [SwaggerRequestBodyType(typeof(RequestBodyModelWithComplexType), true, "RequestModel", "Description")]
        [FunctionName("PostNestedComplexTypeAnnotation")]
        public static async Task<IActionResult> PostNestedComplexTypeAnnotation([HttpTrigger(
                AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Triggered");
            var input = JsonConvert.DeserializeObject<RequestBodyModelWithComplexType>(await req.ReadAsStringAsync());
            return new OkObjectResult(input);
        }

        [SwaggerFormDataFile(true, "file", "Description")]
        [SwaggerFormData("text", false, typeof(string), "Description")]
        [SwaggerFormData("number", false, typeof(int), "Description")]
        [Consumes("multipart/form-data")]
        [FunctionName("MultipartFormUploadAnnotation")]
        public static async Task<IActionResult> MultipartFormUploadAnnotation([HttpTrigger(
            AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Triggered");

            var formData = await MultipartFormReader.Read(req);
            return new OkObjectResult(formData);

        }

        [FunctionName("AuthorizeAnnotation")]
        [SwaggerAuthorize]
        public static async Task<IActionResult> AuthorizeAnnotation([HttpTrigger(
            AuthorizationLevel.Function, "GET", Route = "test/authorizedonly")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Triggered");
            return new OkObjectResult("ok");
        }


    }
}
