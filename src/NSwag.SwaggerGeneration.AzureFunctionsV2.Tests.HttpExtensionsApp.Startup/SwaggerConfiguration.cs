using System.Collections.Generic;
using System.Linq;
using NSwag.Generation.Processors.Security;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Processors;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsApp.Startup
{
    public static class SwaggerConfiguration
    {
        public static AzureFunctionsV2ToSwaggerGeneratorSettings SwaggerGeneratorSettings { get; set; }

        /// <summary>
        /// Initialize SwaggerGenerator configuration.
        /// Add OperationSecurityProcessors and SecurityDefinitionAppenders to the settings.
        /// </summary>
        static SwaggerConfiguration()
        {
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            SwaggerGeneratorSettings = settings;

            settings.Title = "Azure Functions Swagger example";
            settings.Description =
                "This is an example generated Swagger JSON using NSwag.SwaggerGeneration.AzureFunctionsV2 and AzureFunctionsV2.HttpExtensions to " +
                "generate Swagger output directly from the assembly. <br/>Mostly the methods do nothing.<br/><br/>Credentials for testing:<br/><br/><b>OAuth2:</b> " +
                "\"testuser@testcorp.eu\" : \"foobar123---\", use client_id: \"XLjNBiBCx3_CZUAK3gagLSC_PPQjBDzB\"" +
                "<br/><b>Basic auth:</b> \"user\" : \"pass\" <br/> " +
                "<b>ApiKey:</b> \"key\".";

            settings.OperationProcessors.Add(new OperationSecurityProcessor("Bearer",
                OpenApiSecuritySchemeType.OpenIdConnect));
            var scopes = new List<string>() {"openid", "profile", "name"};
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("Bearer", scopes, new OpenApiSecurityScheme()
            {
                Type = OpenApiSecuritySchemeType.OAuth2,
                Flow = OpenApiOAuth2Flow.Implicit,
                AuthorizationUrl = "https://jusas-tests.eu.auth0.com/authorize",
                Scopes = scopes.ToDictionary(x => x, x => x),
                TokenUrl = "https://jusas-tests.eu.auth0.com/oauth/token",
                Description = "Token"
            }));

            // The SecurityDefinitionAppender constructor is not actually obsolete, see
            // https://github.com/RicoSuter/NSwag/pull/2305

            settings.OperationProcessors.Add(new OperationSecurityProcessor("Basic",
                OpenApiSecuritySchemeType.Basic));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("Basic", new OpenApiSecurityScheme()
            {
                Type = OpenApiSecuritySchemeType.Basic,
                Scheme = "Basic",
                Description = "Basic auth"
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("HApiKey",
                OpenApiSecuritySchemeType.ApiKey, OpenApiSecurityApiKeyLocation.Header));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("HApiKey", new OpenApiSecurityScheme()
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "x-apikey",
                In = OpenApiSecurityApiKeyLocation.Header
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("QApiKey",
                OpenApiSecuritySchemeType.ApiKey, OpenApiSecurityApiKeyLocation.Query));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("QApiKey", new OpenApiSecurityScheme()
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "apikey",
                In = OpenApiSecurityApiKeyLocation.Query
            }));
        }
        
    }
}
