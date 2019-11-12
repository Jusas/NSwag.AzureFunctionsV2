using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using NSwag.Annotations;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp
{
    public class NonStaticFunctionClass
    {
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("NonStaticClassFunc")]
        public async Task<IActionResult> NonStaticClassFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req
        )
        {
            return new OkObjectResult("ok");
        }
    }

    public class IgnoredNonStaticFunctionClass
    {
        [OpenApiIgnore]
        [FunctionName("IgnoredNonStaticClassFunc")]
        public async Task<IActionResult> IgnoredNonStaticClassFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req
        )
        {
            return new OkObjectResult("ok");
        }
    }
}
