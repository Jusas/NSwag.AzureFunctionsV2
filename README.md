# NSwag Azure Functions v2 Swagger Generator

[![Wiki](https://img.shields.io/badge/docs-in%20wiki-green.svg?style=flat)](https://github.com/Jusas/NSwag.AzureFunctionsV2/wiki) 
[![Nuget](https://img.shields.io/nuget/v/NSwag.SwaggerGeneration.AzureFunctionsV2.svg)](https://www.nuget.org/packages/NSwag.SwaggerGeneration.AzureFunctionsV2/) 

![Logo](assets/logo.png)

This is an extension to NSwag, specifically a SwaggerGenerator implementation designed
for Azure Functions. This implementation has the necessary discovery methods to scan
assemblies for Functions and the library adds new annotations that allows you to
generate Swagger documentation from the expected Function parameters and return values.

Additionally the Swagger Generator supports the [AzureFunctionsV2.HttpExtensions](https://github.com/Jusas/AzureFunctionsV2.HttpExtensions) library
without any extra developer effort promoting Azure Functions to a first class Swagger citizen,
with Function parameters picked up from function signatures automatically and the additional attribute-based authorization schemes supported and documented automatically. 
No extra annotations are necessary excluding any additional information you wish to convey via
the old and new Swagger annotations.

## Features

- Enables you to generate Swagger Documents out of Azure Function classes
- Provides the necessary new method level annotations to produce the parameter level documentation when HttpExtensions are not used
- Supports [AzureFunctionsV2.HttpExtensions](https://github.com/Jusas/AzureFunctionsV2.HttpExtensions) which allows you to insert request parameters directly into function signature and consequently removes the requirement for having method level parameter annotations

With this you'll be able to have an up-to-date definition with your Functions at all times
with minimal hassle. The main motivator for this library was to enable using HttpTriggered 
Functions more in the manner of a traditional API, and in that mindset having a generated
Swagger definition is a must.

It's worth noting that since Functions do not really behave like ASP.NET Core despite them
having some similarities, which forces us to do our customized method discovery and fill
some gaps with additional annotations.


## Examples

### Basic usage

The basic functionality largely revolves around the new annotations; `SwaggerAuthorizeAttribute`, `SwaggerFormDataAttribute`, `SwaggerFormDataFileAttribute`, `SwaggerQueryParameterAttribute` , `SwaggerRequestBodyTypeAttribute` and `SwaggerRequestHeaderAttribute` which are all applied on the method level. For example:

```C#
[SwaggerQueryParameter("queryParam", false /* not a required param */, typeof(string), "A query parameter")]
[SwaggerResponse(200, typeof(string), Description = "OK result")]
[FunctionName("SwaggerQueryParamAttribute1")]
public static async Task<IActionResult> SwaggerQueryParamAttribute1(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    ILogger log)
{
    return new OkResult();
}
```

Produces:

```JSON
"/api/SwaggerQueryParamAttribute1": {
  "get": {
    "tags": [
      "GenerationAnnotationTests"
    ],
    "operationId": "GenerationAnnotationTests_SwaggerQueryParamAttribute1",
    "parameters": [
      {
        "type": "string",
        "name": "queryParam",
        "in": "query",
        "description": "A query parameter",
        "x-nullable": true
      }
    ],
    "responses": {
      "200": {
        "x-nullable": true,
        "description": "OK result",
        "schema": {
          "type": "string"
        }
      }
    }
  }
}
```

And to actually share the Swagger document with the world, we can create a Function in our Function App that produces and serves it:

```C#
[SwaggerIgnore]
[FunctionName("swagger")]
public static async Task<IActionResult> Swagger(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
    HttpRequest req,
    ILogger log)
{
    var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
    settings.Title = "Azure Functions Swagger example";
    var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);    
    var document = await generator.GenerateForAzureFunctionClassAsync(typeof(MyFunctionApp));
    var json = document.ToJson();
    return new OkObjectResult(json);
}

```

### Usage with AzureFunctionsV2.HttpExtensions

Since the HttpExtensions enables us to put request parameters directly into the
function signature, things become a lot easier since you no longer need to add
any attributes to the method - the parameters are automatically discovered by
the Swagger Generator. In a similar example to the above one, here we have list of
integers here defined as a query parameter, and nothing extra is required to
generate the proper definition as parameters in the signature get picked up automatically.

```C#
[SwaggerResponse(200, typeof(string), Description = "OK response")]
[FunctionName("HttpExtensionsQueryParams2")]
public static async Task<IActionResult> HttpExtensionsQueryParams2(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    [HttpQuery]HttpParam<List<int>> queryParam,
    ILogger log)
{
    return new OkResult();
}
```

Produces:

```JSON
"/api/HttpExtensionsQueryParams2": {
  "get": {
    "tags": [
      "HttpExtensionTests"
    ],
    "operationId": "HttpExtensionTests_HttpExtensionsQueryParams2",
    "parameters": [
      {
        "type": "array",
        "name": "queryParam",
        "in": "query",
        "collectionFormat": "multi",
        "x-nullable": true,
        "items": {
          "type": "integer",
          "format": "int32"
        }
      }
    ],
    "responses": {
      "200": {
        "x-nullable": true,
        "description": "OK response",
        "schema": {
          "type": "string"
        }
      }
    }
  }
}
```

### Advanced usage

Authentication/authorization is also supported in both "Swagger annotations only" and 
"HttpExtensions with Swagger" scenarios. This'll require some additional configuration,
namely you'll need to provide the `AzureFunctionsV2ToSwaggerGeneratorSettings` the 
security schemes you're using. For example:

```C#
settings.DocumentProcessors.Add(
  new SecurityDefinitionAppender("MyBasicAuth", new SwaggerSecurityScheme()
{
    Type = SwaggerSecuritySchemeType.Basic,
    Scheme = "Basic",
    Description = "Basic auth"
}));
settings.OperationProcessors.Add(
  new OperationSecurityProcessor("MyBasicAuth", SwaggerSecuritySchemeType.Basic));
```

And with this, the following Function annotation will properly be documented and
accessible via __Swagger UI__ as well:

```C#
[SwaggerAuthorize(AuthScheme.Basic)]
[SwaggerResponse(200, typeof(string), Description = "OK result")]
[FunctionName("SwaggerAuthorizeAttribute1")]
public static async Task<IActionResult> SwaggerAuthorizeAttribute1(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    ILogger log)
{
    return new OkResult();
}
```

This produces:

```JSON
"/api/SwaggerAuthorizeAttribute1": {
  "get": {
    "tags": [
      "GenerationAnnotationTests"
    ],
    "operationId": "GenerationAnnotationTests_SwaggerAuthorizeAttribute1",
    "responses": {
      "200": {
        "x-nullable": true,
        "description": "OK result",
        "schema": {
          "type": "string"
        }
      }
    },
    "security": [
      {
        "MyBasicAuth": []
      }
    ]
  }
},
...
"securityDefinitions": {
  "MyBasicAuth": {
    "type": "basic",
    "description": "Basic auth"
  }
}
```

So as a summary, you declared a security scheme called "Basic", with the type being 
`SwaggerSecuritySchemeType.Basic` (this produces the security definition at the end of the 
JSON), and the an `OperationSecurityProcessor` (which gets run for all the Functions) that
you tell to bind Basic authentication schemes to a security definition called "Basic",
resulting in the above Swagger JSON.

More advanced examples (including OAuth2 and ApiKey configurations) can be found from the [wiki](https://github.com/Jusas/NSwag.NSwag.AzureFunctionsV2/wiki).


## Known issues

The XML documentation reading is unfortunately not working correctly at the moment.
It works locally, provided that you have enabled the XML documentation generation
from the project settings and you copy the generated XML doc to your binaries folder.
However in Azure the file paths seem to generate some problems for the time being.
