using System;
using System.Collections.Generic;
using System.Text;

namespace NSwag.Annotations.AzureFunctionsV2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerQueryParameterAttribute : Attribute
    {
        public SwaggerQueryParameterAttribute(string name, bool required = false, Type type = null, string description = null)
        {
            Name = name;
            Description = description;
            Type = type ?? typeof(string);
            Required = required;
        }

        /// <summary>
        /// The name of the query parameter in Swagger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the query parameter data.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The Swagger description of the query parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the header is required or not.
        /// </summary>
        public bool Required { get; set; }
    }
}
