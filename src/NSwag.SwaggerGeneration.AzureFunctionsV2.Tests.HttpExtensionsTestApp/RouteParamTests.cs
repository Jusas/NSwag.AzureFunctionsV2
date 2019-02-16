using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp
{
    public static class RouteParamTests
    {
        [FunctionName("RouteParamTest")]
        public static async Task<IActionResult> RouteParamTest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/{num}/{str}")] HttpRequest req,
            int num,
            string str,
            ILogger log)
        {
            return new OkResult();
        }
    }
}
