# NSwag Azure Functions v2 Swagger Generator

[![Wiki](https://img.shields.io/badge/docs-in%20wiki-green.svg?style=flat)](https://github.com/Jusas/NSwag.AzureFunctionsV2/wiki) 
[![Nuget](https://img.shields.io/nuget/v/NSwag.SwaggerGeneration.AzureFunctionsV2.svg)](https://www.nuget.org/packages/NSwag.SwaggerGeneration.AzureFunctionsV2/) 
[![NSwag support](https://img.shields.io/badge/NSwag%20version%20tested-12.0.14-blue.svg)](https://github.com/RicoSuter/NSwag)
[![NSwag support](https://img.shields.io/badge/NSwag%20version%20tested-13.0.4-blue.svg)](https://github.com/RicoSuter/NSwag)

![Logo](assets/logo.png)

## Latest version

Latest published version is __v1.1.1___.

Changes:

* Added support for non-static Function classes


## Supported versions of NSwag

* NSwag version __v12.0.14__ has been tested and is supported by this library __v1.0.1__
* NSwag version __v13.0.4__  has been tested and is supported by this library __v1.1.*__

**Please note that v13 of NSwag introduces major naming changes and refactorings**.
Most names now use __OpenApi__ prefixes instead of __Swagger__ so some refactoring will be needed
on the using side as well. This library however has made minimal changes to namings when adapting v13 of NSwag and
for the most part hides NSwag behind it so you should get away with minimal changes.

Also note that since v13 the main dependency has changed from NSwag.SwaggerGeneration to NSwag.Generation.

## Introduction

**See the demo!** https://functionsswagger.azurewebsites.net/api/swaggerui/index.html (an Azure Function App serving a Swagger UI and the Swagger JSON that is generated on the fly from the Function App assembly).    

The demo source is in two projects: [HttpExtensionsTestApp](src/NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsTestApp) and [HttpExtensionsTestApp.Startup](src/NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsApp.Startup).

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

## NSwag Core version compatibility

NSwag.AzureFunctionsV2 is built as a Swagger Generator implementation on top of NSwag.Core and NSwag.Generation packages and it's important to note that 
these projects aren't necessarily updated in sync. Hence why specific versions of this library support specific versions of NSwag. The development of NSwag
is quite fluid and the API changes a lot, and it does not follow strict semantic versioning so it's quite possible that a minor and patch version updates can be
breaking. That is why the supported, tested compatible versions are explicitly listed.

The shield on top (![Shield](https://img.shields.io/badge/NSwag%20version%20tested-grey.svg)) tells you the version of NSwag that the current version of 
NSwag.AzureFunctionsV2 has been developed and tested with. Most of the time however the latest NSwag 12.0.xx patch versions should be compatible with v1.0.1 though, 
but it is possible that a public API change in the NSwag projects slips in to a patch version and breaks something. At the time of writing, compatibility update v1.1.0
brings support for NSwag v13, and has been tested against v13.0.4.

Please note that the naming scheme has changed between NSwag v12 and v13, and classes starting with Swagger in v12 are for the most part been renamed to start with OpenApi in v13.

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
[OpenApiIgnore]
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
  new SecurityDefinitionAppender("MyBasicAuth", new OpenApiSecurityScheme()
{
    Type = OpenApiSecuritySchemeType.Basic,
    Scheme = "Basic",
    Description = "Basic auth"
}));
settings.OperationProcessors.Add(
  new OperationSecurityProcessor("MyBasicAuth", OpenApiSecuritySchemeType.Basic));
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
`OpenApiSecuritySchemeType.Basic` (this produces the security definition at the end of the 
JSON), and the an `OperationSecurityProcessor` (which gets run for all the Functions) that
you tell to bind Basic authentication schemes to a security definition called "Basic",
resulting in the above Swagger JSON.

More advanced examples (including OAuth2 and ApiKey configurations) can be found from the [wiki](https://github.com/Jusas/NSwag.AzureFunctionsV2/wiki).


## Known issues

The XML documentation reading is unfortunately not working correctly at the moment.
It works locally, provided that you have enabled the XML documentation generation
from the project settings and you copy the generated XML doc to your binaries folder.
However in Azure the file paths seem to generate some problems for the time being.
