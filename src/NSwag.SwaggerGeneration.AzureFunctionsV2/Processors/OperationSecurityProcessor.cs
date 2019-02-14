﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NJsonSchema.Infrastructure;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using NSwag.SwaggerGeneration.Processors.Security;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Processors
{
    public class OperationSecurityProcessor : IOperationProcessor
    {

        private readonly string _name;
        private SwaggerSecuritySchemeType _securitySchemeType;
        private SwaggerSecurityApiKeyLocation? _location;

        public OperationSecurityProcessor(string name, SwaggerSecuritySchemeType type, SwaggerSecurityApiKeyLocation? location = null)
        {
            _name = name;
            _securitySchemeType = type;
            _location = location;
        }

        /// <summary>Processes the specified method information.</summary>
        /// <param name="context"></param>
        /// <returns>true if the operation should be added to the Swagger specification.</returns>
        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var securityRequirement = GetSecurityRequirement(context.OperationDescription, context.MethodInfo);

            if (securityRequirement)
            {
                context.OperationDescription.Operation.Security.Add(new SwaggerSecurityRequirement()
                {
                    {_name, new string[] { }}
                });
            }

            return Task.FromResult(true);
        }

        private bool GetSecurityRequirement(SwaggerOperationDescription operationDescription, MethodInfo methodInfo)
        {
            var allAttributes = methodInfo.GetCustomAttributes().Concat(
                methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes());

            var authorizeAttributes = allAttributes.Where(a => a.GetType().Name == "SwaggerAuthorizeAttribute").ToList();
            
            // HttpExtensions support.
            var httpExtensionsAuthAttributes = allAttributes.Where(
                a => a.GetType().Name == "HttpAuthorizeAttribute" ||
                     a.GetType().InheritsFrom("HttpAuthorizeAttribute", TypeNameStyle.Name)).ToList();

            if (_securitySchemeType == SwaggerSecuritySchemeType.Basic)
            {
                var basicAuthRequirement = httpExtensionsAuthAttributes.Any(x =>
                    x.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "Basic") ||
                    authorizeAttributes.Any(a => a.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "Basic");
                return basicAuthRequirement;
            }
            if (_securitySchemeType == SwaggerSecuritySchemeType.ApiKey && _location == SwaggerSecurityApiKeyLocation.Header)
            {
                var apiKeyAuthRequirement = httpExtensionsAuthAttributes.Any(x =>
                    x.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "HeaderApiKey") ||
                    authorizeAttributes.Any(a => a.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "HeaderApiKey");
                return apiKeyAuthRequirement;
            }
            if (_securitySchemeType == SwaggerSecuritySchemeType.ApiKey && _location == SwaggerSecurityApiKeyLocation.Query)
            {
                var apiKeyAuthRequirement = httpExtensionsAuthAttributes.Any(x =>
                    x.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "QueryApiKey") ||
                    authorizeAttributes.Any(a => a.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "QueryApiKey");
                return apiKeyAuthRequirement;
            }
            if (_securitySchemeType == SwaggerSecuritySchemeType.OAuth2 ||
                     _securitySchemeType == SwaggerSecuritySchemeType.OpenIdConnect)
            {
                var bearerRequirement = httpExtensionsAuthAttributes.Any(x =>
                    new[] { "OAuth2", "Jwt" }.Contains(x.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString())) ||
                    authorizeAttributes.Any(a => a.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "Bearer");
                return bearerRequirement;
            }

            return false;
        }

    }
}