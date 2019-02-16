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
        /// <summary>
        /// Initializes the <see cref="SwaggerAuthorizeAttribute"/> with Basic AuthScheme.
        /// </summary>
        public SwaggerAuthorizeAttribute()
        {
            Scheme = AuthScheme.Basic;
        }

        /// <summary>
        /// Initializes the <see cref="SwaggerAuthorizeAttribute"/> with the given AuthScheme.
        /// </summary>
        /// <param name="scheme"></param>
        public SwaggerAuthorizeAttribute(AuthScheme scheme)
        {
            Scheme = scheme;
        }
        
        /// <summary>
        /// The authorization scheme.
        /// </summary>
        public AuthScheme Scheme { get; set; }

    }

}
