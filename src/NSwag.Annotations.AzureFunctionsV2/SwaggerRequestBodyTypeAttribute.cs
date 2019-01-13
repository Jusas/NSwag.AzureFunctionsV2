using System;

namespace NSwag.Annotations.AzureFunctionsV2
{
    /// <summary>
    /// Indicates the type of the request body.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerRequestBodyTypeAttribute : Attribute
    {
        /// <summary>
        /// Instantiates the attribute.
        /// </summary>
        /// <param name="type">The type of the data expected in request body</param>
        /// <param name="required">Indicates whether the body is required or not</param>
        /// <param name="name">The name of the body parameter in Swagger</param>
        /// <param name="description">The Swagger description of the body</param>
        public SwaggerRequestBodyTypeAttribute(Type type, bool required = false, string name = null, string description = null)
        {
            Type = type;
            Name = name;
            Description = description;
            Required = required;
        }

        /// <summary>
        /// The type of the data expected in request body.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The name of the body parameter in Swagger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Swagger description of the body.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the body is required or not.
        /// </summary>
        public bool Required { get; set; }
    }
}
