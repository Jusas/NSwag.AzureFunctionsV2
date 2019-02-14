using System;
using System.Collections.Generic;
using System.Text;

namespace NSwag.Annotations.AzureFunctionsV2
{
    public enum AuthScheme
    {
        Basic,
        Bearer,
        HeaderApiKey,
        QueryApiKey
    }
    /// <summary>
    /// Authorize annotation. NOTE! Only an annotation! Does not secure the method in any way!
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerAuthorizeAttribute : Attribute
    {
        public SwaggerAuthorizeAttribute()
        {
            Scheme = AuthScheme.Basic;
        }

        public SwaggerAuthorizeAttribute(AuthScheme scheme)
        {
            Scheme = scheme;
        }
        
        public AuthScheme Scheme { get; set; }

    }

}
