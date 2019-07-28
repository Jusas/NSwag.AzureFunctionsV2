using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Namotion.Reflection;
using NJsonSchema.Infrastructure;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.Processors.Security;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Processors
{
    /// <summary>
    /// Security processor which scans methods for security attributes/annotations.
    /// <para>
    /// This should be instantiated per different security scheme (ie. Basic, OAuth2, ApiKey...) you're using.
    /// </para>
    /// </summary>
    public class OperationSecurityProcessor : IOperationProcessor
    {

        private readonly string _name;
        private OpenApiSecuritySchemeType _securitySchemeType;
        private OpenApiSecurityApiKeyLocation? _location;

        /// <summary>
        /// Initializes the <see cref="OperationSecurityProcessor"/> with the given name (which should match the name of
        /// your <see cref="SecurityDefinitionAppender"/>) and <see cref="OpenApiSecuritySchemeType"/>.
        /// </summary>
        /// <param name="name">The name of your Swagger security definition (which should match the name you gave to
        /// your <see cref="SecurityDefinitionAppender"/>).</param>
        /// <param name="type">The type of the scheme.</param>
        /// <param name="location">If the type was <see cref="OpenApiSecuritySchemeType.ApiKey"/>, the expected location of the key, otherwise null.</param>
        public OperationSecurityProcessor(string name, OpenApiSecuritySchemeType type, OpenApiSecurityApiKeyLocation? location = null)
        {
            _name = name;
            _securitySchemeType = type;
            _location = location;
        }

        /// <summary>Processes the specified method information.</summary>
        /// <param name="context"></param>
        /// <returns>true if the operation should be added to the Swagger specification.</returns>
        public bool Process(OperationProcessorContext context)
        {
            var securityRequirement = GetSecurityRequirement(context.MethodInfo);

            if (securityRequirement)
            {
                if(context.OperationDescription.Operation.Security == null)
                    context.OperationDescription.Operation.Security = new Collection<OpenApiSecurityRequirement>();

                context.OperationDescription.Operation.Security.Add(new OpenApiSecurityRequirement()
                {
                    {_name, new string[] { }}
                });
            }

            return true;
        }

        private bool GetSecurityRequirement(MethodInfo methodInfo)
        {
            var allAttributes = methodInfo.GetCustomAttributes().Concat(
                methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes());

            var authorizeAttributes = allAttributes.Where(a => a.GetType().Name == "SwaggerAuthorizeAttribute").ToList();
            
            // HttpExtensions support.
            var httpExtensionsAuthAttributes = allAttributes.Where(
                a => a.GetType().Name == "HttpAuthorizeAttribute" ||
                     a.GetType().InheritsFromTypeName("HttpAuthorizeAttribute", TypeNameStyle.Name)).ToList();

            if (_securitySchemeType == OpenApiSecuritySchemeType.Basic)
            {
                var basicAuthRequirement = httpExtensionsAuthAttributes.Any(x =>
                    x.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "Basic") ||
                    authorizeAttributes.Any(a => a.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "Basic");
                return basicAuthRequirement;
            }
            if (_securitySchemeType == OpenApiSecuritySchemeType.ApiKey && (_location == OpenApiSecurityApiKeyLocation.Header || _location == null))
            {
                var apiKeyAuthRequirement = httpExtensionsAuthAttributes.Any(x =>
                    x.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "HeaderApiKey") ||
                    authorizeAttributes.Any(a => a.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "HeaderApiKey");
                return apiKeyAuthRequirement;
            }
            if (_securitySchemeType == OpenApiSecuritySchemeType.ApiKey && _location == OpenApiSecurityApiKeyLocation.Query)
            {
                var apiKeyAuthRequirement = httpExtensionsAuthAttributes.Any(x =>
                    x.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "QueryApiKey") ||
                    authorizeAttributes.Any(a => a.TryGetPropertyValue<Enum>("Scheme", default(Enum)).ToString() == "QueryApiKey");
                return apiKeyAuthRequirement;
            }
            if (_securitySchemeType == OpenApiSecuritySchemeType.OAuth2 ||
                     _securitySchemeType == OpenApiSecuritySchemeType.OpenIdConnect)
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
