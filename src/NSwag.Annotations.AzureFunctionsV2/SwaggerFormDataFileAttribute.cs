using System;

namespace NSwag.Annotations.AzureFunctionsV2
{
    /// <summary>
    /// Indicates that the request body of this method is a file (multipart/form-data).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerFormDataFileAttribute : Attribute
    {
        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="multiFile">Indicates whether the method supports multiple file uploads</param>
        /// <param name="name">The name of the file parameter</param>
        /// <param name="description">The Swagger description of the file field</param>
        public SwaggerFormDataFileAttribute(bool multiFile = false, string name = null, string description = null)
        {
            Name = name;
            MultiFile = multiFile;
            Description = description;
        }
        
        /// <summary>
        /// The name of the file parameter in Swagger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the method supports multi-file upload.
        /// </summary>
        public bool MultiFile { get; set; }

        /// <summary>
        /// The Swagger description of the file field.
        /// </summary>
        public string Description { get; set; }
    }
}
