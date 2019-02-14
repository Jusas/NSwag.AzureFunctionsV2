using System.Collections.Generic;
using System.Security.Claims;
using AzureFunctionsV2.HttpExtensions.Authorization;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsApp.Startup;

[assembly: WebJobsStartup(typeof(Startup), "MyStartup")]

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.HttpExtensionsApp.Startup
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.Configure<HttpAuthenticationOptions>(options =>
            {
                options.ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (s, request) => s == "key" ? true : false,
                    HeaderName = "x-apikey",
                    QueryParameterName = "apikey"
                };
                options.BasicAuthentication = new BasicAuthenticationParameters()
                {
                    ValidCredentials = new Dictionary<string, string>() { { "user", "pass" } }
                };
                options.JwtAuthentication = new JwtAuthenticationParameters()
                {
                    TokenValidationParameters = new OpenIdConnectJwtValidationParameters()
                    {
                        OpenIdConnectConfigurationUrl =
                            "https://jusas-tests.eu.auth0.com/.well-known/openid-configuration",
                        ValidAudiences = new List<string>()
                            {"XLjNBiBCx3_CZUAK3gagLSC_PPQjBDzB"},
                        ValidateIssuerSigningKey = true,
                        NameClaimType = ClaimTypes.NameIdentifier
                    },
                    AuthorizationFilter = async (principal, token, attributes) => { }
                };
            });
            

        }
    }
}
