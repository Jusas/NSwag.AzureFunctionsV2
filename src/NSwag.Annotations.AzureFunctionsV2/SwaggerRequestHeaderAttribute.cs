using System;
using System.Collections.Generic;
using System.Text;

namespace NSwag.Annotations.AzureFunctionsV2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerRequestHeaderAttribute : Attribute
    {
        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <param name="required">Indicates whether the header is required or not</param>
        /// <param name="type">The type of the header data</param>
        /// <param name="description">The Swagger description of the header</param>
        public SwaggerRequestHeaderAttribute(string name, bool required = false, Type type = null, string description = null)
        {
            Name = name;
            Description = description;
            Type = type ?? typeof(string);
            Required = required;
        }

        /// <summary>
        /// The name of the header parameter in Swagger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the header data.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The Swagger description of the header.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the header is required or not.
        /// </summary>
        public bool Required { get; set; }
    }
}
