using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Processors
{
    public class OperationSecurityScopeProcessor : IOperationProcessor
    {
        private readonly string _name;

        /// <summary>Initializes a new instance of the <see cref="OperationSecurityScopeProcessor"/> class with 'Bearer' name.</summary>
        public OperationSecurityScopeProcessor() : this("Bearer")
        {
        }

        /// <summary>Initializes a new instance of the <see cref="OperationSecurityScopeProcessor"/> class.</summary>
        /// <param name="name">The security definition name.</param>
        public OperationSecurityScopeProcessor(string name)
        {
            _name = name;
        }

        /// <summary>Processes the specified method information.</summary>
        /// <param name="context"></param>
        /// <returns>true if the operation should be added to the Swagger specification.</returns>
        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var scopes = GetScopes(context.OperationDescription, context.MethodInfo);
            if (scopes.Any())
            {
                if (context.OperationDescription.Operation.Security == null)
                    context.OperationDescription.Operation.Security = new List<SwaggerSecurityRequirement>();
                context.OperationDescription.Operation.Security.Add(new SwaggerSecurityRequirement
                {
                    { _name, scopes }
                });
            }

            return Task.FromResult(true);
        }

        /// <summary>Gets the security scopes for an operation.</summary>
        /// <param name="operationDescription">The operation description.</param>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>The scopes.</returns>
        protected virtual IEnumerable<string> GetScopes(SwaggerOperationDescription operationDescription, MethodInfo methodInfo)
        {
            var allAttributes = methodInfo.GetCustomAttributes().Concat(
                methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes());

            var authorizeAttributes = allAttributes.Where(a => a.GetType().Name == "SwaggerAuthorizeAttribute").ToList();
            // HttpExtensions support.
            var httpExtensionsAuthAttributes = allAttributes.Where(a => a.GetType().Name == "HttpJwtAuthorizeAttribute").ToList();

            if (!authorizeAttributes.Any() && !httpExtensionsAuthAttributes.Any())
                return Enumerable.Empty<string>();

            var distinctRoles = authorizeAttributes
                .Select(a => (dynamic)a)
                .Where(a => a.Roles != null)
                .SelectMany(a => ((string)a.Roles).Split(','))
                .Distinct()
                .ToList();

            if(httpExtensionsAuthAttributes.Any())
                distinctRoles.Add("");

            return distinctRoles.Distinct();

        }
    }
}
