using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp
{
    public static class HttpExtensionTests
    {
        public class Dog
        {
            public string Name { get; set; }
            public bool IsGoodBoy { get; set; }
        }

        /// <summary>
        /// Query parameter of type string, name 'qp'
        /// </summary>
        /// <param name="req"></param>
        /// <param name="queryParam"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsQueryParams1")]
        public static async Task<IActionResult> HttpExtensionsQueryParams1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [HttpQuery("qp", Required = true)]HttpParam<string> queryParam,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Query param of type List&lt;string&gt;, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="queryParam"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsQueryParams2")]
        public static async Task<IActionResult> HttpExtensionsQueryParams2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [HttpQuery]HttpParam<List<int>> queryParam,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Query param of type Dog
        /// </summary>
        /// <param name="req"></param>
        /// <param name="queryParam"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsQueryParams3")]
        public static async Task<IActionResult> HttpExtensionsQueryParams3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [HttpQuery]HttpParam<Dog> queryParam,
            ILogger log)
        {
            return new OkResult();
        }
    }
}
