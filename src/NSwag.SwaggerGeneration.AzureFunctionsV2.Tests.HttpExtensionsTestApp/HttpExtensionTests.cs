using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequest req,
            [HttpQuery("qp", Required = true)]HttpParam<string> queryParam,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Query param of type List&lt;int&gt;, not required
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

        /// <summary>
        /// Header of type string, x-header, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="header"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsHeaders1")]
        public static async Task<IActionResult> HttpExtensionsHeaders1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [HttpHeader(Name = "x-header")]HttpParam<string> header,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Header of type Dog, x-header, required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="header"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsHeaders2")]
        public static async Task<IActionResult> HttpExtensionsHeaders2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [HttpHeader(Name = "x-header", Required = true)]HttpParam<Dog> header,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Body of type Dog, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="body"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsBody1")]
        public static async Task<IActionResult> HttpExtensionsBody1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [HttpBody]HttpParam<Dog> body,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Body of type string, required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="body"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsBody2")]
        public static async Task<IActionResult> HttpExtensionsBody2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = null)] HttpRequest req,
            [HttpBody(Required = true)]HttpParam<string> body,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Body of type Dog[], required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="body"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsBody3")]
        public static async Task<IActionResult> HttpExtensionsBody3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = null)] HttpRequest req,
            [HttpBody(Required = true)]HttpParam<Dog[]> body,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Body of type XmlDocument
        /// </summary>
        /// <param name="req"></param>
        /// <param name="body"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsBody4")]
        public static async Task<IActionResult> HttpExtensionsBody4(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [HttpBody(Required = true)]HttpParam<XmlDocument> body,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// FormData of type string, required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="formField"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsForm1")]
        public static async Task<IActionResult> HttpExtensionsForm1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [HttpForm(Required = true)]HttpParam<string> formField,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// FormData of type Dog, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="formField"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsForm2")]
        public static async Task<IActionResult> HttpExtensionsForm2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [HttpForm(Required = true)]HttpParam<Dog> formField,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// FormData with IFormFile (required) and a string form field
        /// </summary>
        /// <param name="req"></param>
        /// <param name="file"></param>
        /// <param name="formField"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HttpExtensionsForm3")]
        public static async Task<IActionResult> HttpExtensionsForm3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [HttpForm(Required = true)]HttpParam<IFormFile> file,
            [HttpForm]HttpParam<string> formField,
            ILogger log)
        {
            return new OkResult();
        }

    }
}
