using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSwag.Annotations;
using NSwag.Annotations.AzureFunctionsV2;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp
{
    public static class GenerationAnnotationTests
    {
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }



        /// <summary>
        /// Default SwaggerAuthorizeAttribute
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerAuthorize]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerAuthorizeAttribute1")]
        public static async Task<IActionResult> SwaggerAuthorizeAttribute1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerAuthorizeAttribute with header api key
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerAuthorize(AuthScheme.HeaderApiKey)]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerAuthorizeAttribute2")]
        public static async Task<IActionResult> SwaggerAuthorizeAttribute2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerAuthorizeAttribute with query api key
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerAuthorize(AuthScheme.QueryApiKey)]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerAuthorizeAttribute3")]
        public static async Task<IActionResult> SwaggerAuthorizeAttribute3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerFormDataAttribute that is required, is string
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerFormData("formField1", true, typeof(string), "description")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerFormDataAttribute1")]
        public static async Task<IActionResult> SwaggerFormDataAttribute1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerFormDataFile that is single file
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerFormDataFile(false, "file", "description")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerFormDataFileAttribute1")]
        public static async Task<IActionResult> SwaggerFormDataFileAttribute1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerFormDataFile that is multifile
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerFormDataFile(true, "files", "description")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerFormDataFileAttribute2")]
        public static async Task<IActionResult> SwaggerFormDataFileAttribute2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerQueryParamAttribute that is string, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerQueryParameter("queryParam", false, typeof(string), "A query parameter")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerQueryParamAttribute1")]
        public static async Task<IActionResult> SwaggerQueryParamAttribute1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerQueryParamAttribute that is int, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerQueryParameter("queryParam", false, typeof(int), "Yet another description")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerQueryParamAttribute2")]
        public static async Task<IActionResult> SwaggerQueryParamAttribute2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerQueryParamAttribute that is list of int, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerQueryParameter("queryParam", false, typeof(List<int>))]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerQueryParamAttribute3")]
        public static async Task<IActionResult> SwaggerQueryParamAttribute3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerRequestBodyTypeAttribute that is Person, required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerRequestBodyType(typeof(Person), Description = "description", Required = true, Name = "Body")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerRequestBodyTypeAttribute1")]
        public static async Task<IActionResult> SwaggerRequestBodyTypeAttribute1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// SwaggerRequestHeaderAttribute that is string, not required
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerRequestHeader("x-header", false, typeof(string), "description")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerRequestHeaderAttribute1")]
        public static async Task<IActionResult> SwaggerRequestHeaderAttribute1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Function annotated with SwaggerIgnore.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerIgnore]
        [FunctionName("SwaggerIgnoredFunction1")]
        public static async Task<IActionResult> SwaggerIgnoredFunction1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Function annotated with OpenApiIgnore.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [OpenApiIgnore]
        [FunctionName("SwaggerIgnoredFunction2")]
        public static async Task<IActionResult> SwaggerIgnoredFunction2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        // Derived HttpRequest, should work.
        public class MyHttpPostModel
        {
            public string Something { get; set; }
        }

        /// <summary>
        /// A class with non HttpRequest req parameter.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerFunctionWithNonHttpRequestParam")]
        public static async Task<IActionResult> SwaggerFunctionWithNonHttpRequestParam(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] MyHttpPostModel req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// A class with non HttpRequest req parameter.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerAuthorize]
        [SwaggerRequestBodyType(typeof(MyHttpPostModel), false, "My Custom Body", "My Custom Body Description")]
        [SwaggerResponse(200, typeof(string), Description = "OK result")]
        [FunctionName("SwaggerFunctionWithNonHttpRequestParamAndBodyTypeSet")]
        public static async Task<IActionResult> SwaggerFunctionWithNonHttpRequestParamAndBodyTypeSet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] MyHttpPostModel req,
            ILogger log)
        {
            return new OkResult();
        }
    }
}
