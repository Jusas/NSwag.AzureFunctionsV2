using System;

namespace NSwag.Annotations.AzureFunctionsV2
{   
    /// <summary>
    /// Indicates that the request body of this method contains a form multi-part key-value pair.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerFormDataAttribute : Attribute
    {
        public SwaggerFormDataAttribute(string name, bool required = false, Type type = null, string description = null)
        {
            Name = name;
            Description = description;
            Type = type ?? typeof(string);
            Required = required;
        }

        /// <summary>
        /// The name of the form field in Swagger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the form field data.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The Swagger description of the form field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the form field is required or not.
        /// </summary>
        public bool Required { get; set; }
    }
}