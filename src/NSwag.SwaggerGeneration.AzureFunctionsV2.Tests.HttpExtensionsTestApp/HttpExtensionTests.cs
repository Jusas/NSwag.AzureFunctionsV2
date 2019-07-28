using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp
{
    public static class HttpExtensionTests
    {
        public class Dog
        {
            public string Name { get; set; }
            public bool IsGoodBoy { get; set; }
        }

        public class MyAuthorizeAttribute : HttpAuthorizeAttribute
        {
            public string Role { get; set; }

            public MyAuthorizeAttribute(Scheme scheme) : base(Scheme.Jwt)
            {
            }

            public MyAuthorizeAttribute() : base(Scheme.Jwt)
            {
            }
        }

        /// <summary>
        /// Query parameter of type string, name 'qp'
        /// </summary>
        /// <param name="req"></param>
        /// <param name="queryParam">A query parameter</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="queryParam">Yet another useless parameter</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="queryParam">Query parameter again</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="header">A pointless header</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="header">Yet another test header</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="body">A dog!</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="body">It's a string!</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="body">An array of doges!</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="body">An XML document</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="formField">A string form field</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="formField">A Doge form field!</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
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
        /// <param name="file">A file form field</param>
        /// <param name="formField">A string form field again</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
        [FunctionName("HttpExtensionsForm3")]
        public static async Task<IActionResult> HttpExtensionsForm3(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [HttpForm(Required = true)]HttpParam<IFormFile> file,
            [HttpForm]HttpParam<string> formField,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// HttpAuthorize attributed (JWT) Function.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(401, typeof(object), Description = "Unauthorized")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
        [HttpAuthorize(Scheme.Jwt)]
        [FunctionName("HttpExtensionsJwtAuth1")]
        public static async Task<IActionResult> HttpExtensionsJwtAuth1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult(new
            {
                user = user.ClaimsPrincipal.Identity.Name
            });
        }

        /// <summary>
        /// HttpAuthorize-inheriting (JWT) attribute attributed Function.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(401, typeof(object), Description = "Unauthorized")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
        [MyAuthorize(Role = "admin")]
        [FunctionName("HttpExtensionsJwtAuth2")]
        public static async Task<IActionResult> HttpExtensionsJwtAuth2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// HttpAuthorize attributed (Basic) Function.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(401, typeof(object), Description = "Unauthorized")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
        [HttpAuthorize(Scheme.Basic)]
        [FunctionName("HttpExtensionsBasicAuth1")]
        public static async Task<IActionResult> HttpExtensionsBasicAuth1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// HttpAuthorize attributed (ApiKey in header) Function.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(401, typeof(object), Description = "Unauthorized")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
        [HttpAuthorize(Scheme.HeaderApiKey)]
        [FunctionName("HttpExtensionsApiKeyAuth1")]
        public static async Task<IActionResult> HttpExtensionsApiKeyAuth1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// HttpAuthorize attributed (ApiKey in query param) Function.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerResponse(200, typeof(string), Description = "OK response")]
        [SwaggerResponse(400, typeof(object), Description = "Bad request, check your input")]
        [SwaggerResponse(401, typeof(object), Description = "Unauthorized")]
        [SwaggerResponse(500, typeof(object), Description = "Server went bonkers")]
        [HttpAuthorize(Scheme.QueryApiKey)]
        [FunctionName("HttpExtensionsApiKeyAuth2")]
        public static async Task<IActionResult> HttpExtensionsApiKeyAuth2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkResult();
        }
    }
}
