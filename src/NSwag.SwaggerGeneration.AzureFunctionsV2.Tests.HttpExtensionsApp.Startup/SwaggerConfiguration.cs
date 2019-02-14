using System.Collections.Generic;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Processors;
using NSwag.SwaggerGeneration.Processors.Security;

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

            settings.OperationProcessors.Add(new OperationSecurityProcessor("Bearer",
                SwaggerSecuritySchemeType.OpenIdConnect));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("Bearer", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.OAuth2,
                Flow = SwaggerOAuth2Flow.Implicit,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = "https://jusas-tests.eu.auth0.com/authorize",
                        Scopes = new Dictionary<string, string>()
                                {{"openid", "openid"}, {"profile", "profile"}, {"name", "name"}},
                        TokenUrl = "https://jusas-tests.eu.auth0.com/oauth/token"
                    }
                },
                Description = "Token"
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("Basic",
                SwaggerSecuritySchemeType.Basic));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("Basic", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.Basic,
                Scheme = "Basic",
                Description = "Basic auth"
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("HApiKey",
                SwaggerSecuritySchemeType.ApiKey, SwaggerSecurityApiKeyLocation.Header));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("HApiKey", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.ApiKey,
                Name = "x-apikey",
                In = SwaggerSecurityApiKeyLocation.Header
            }));

            settings.OperationProcessors.Add(new OperationSecurityProcessor("QApiKey",
                SwaggerSecuritySchemeType.ApiKey, SwaggerSecurityApiKeyLocation.Query));
            settings.DocumentProcessors.Add(new SecurityDefinitionAppender("QApiKey", new SwaggerSecurityScheme()
            {
                Type = SwaggerSecuritySchemeType.ApiKey,
                Name = "apikey",
                In = SwaggerSecurityApiKeyLocation.Query
            }));
        }
        
    }
}
