using System;
using System.Collections.Generic;
using System.Text;

namespace NSwag.Annotations.AzureFunctionsV2
{
    /// <summary>
    /// Authorize annotation. NOTE! Only an annotation! Does not secure the method in any way!
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerAuthorizeAttribute : Attribute
    {
        public SwaggerAuthorizeAttribute(string policy = "", string roles = "")
        {
            Roles = roles;
            Policy = policy;
        }

        public string Roles { get; set; }
        public string Policy { get; set; }
    }

    // TODO: when these are encountered, an instance of Authorize-attribute must be instantiated for the swagger generator to function correctly.
}
