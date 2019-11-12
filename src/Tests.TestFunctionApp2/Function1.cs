using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSwag.AzureFunctionsV2.Extensions.Infrastructure;
using FromQueryAttribute = NSwag.AzureFunctionsV2.Extensions.Annotations.FromQueryAttribute;
using FromHeaderAttribute = NSwag.AzureFunctionsV2.Extensions.Annotations.FromHeaderAttribute;
using FromFormAttribute = NSwag.AzureFunctionsV2.Extensions.Annotations.FromFormAttribute;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        [FunctionName("QueryParams")]
        public static async Task<IActionResult> QueryParams(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [FromQuery(Required = true)]HttpParam<int> number,
            [FromQuery]HttpParam<List<long>> numberList,
            [FromQuery]HttpParam<string> str,
            [FromQuery]HttpParam<List<string>> strList,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"{number.Value}");
            log.LogInformation($"{numberList.Value}");
            log.LogInformation($"{str.Value}");
            log.LogInformation($"{strList.Value}");

            return new OkObjectResult("");
        }

        [FunctionName("Headers")]
        public static async Task<IActionResult> Headers(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [FromHeader("x-my-header")]HttpParam<string> hdr,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation($"{hdr.Value}");
            
            return new OkObjectResult("");
        }

    }
}
