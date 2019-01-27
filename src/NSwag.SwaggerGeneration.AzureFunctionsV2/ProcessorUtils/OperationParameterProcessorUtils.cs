using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.ProcessorUtils
{
    public static class OperationParameterProcessorUtils
    {
        public class SwaggerAzureFunctionAttributeCollection
        {
            public Attribute RequestBodyTypeAttribute { get; set; }
            public Attribute[] HeaderAttributes { get; set; }
            public Attribute[] QueryAttributes { get; set; }
            public Attribute[] UploadFileAttributes { get; set; }
            public Attribute[] FormDataAttributes { get; set; }
        }

        /// <summary>
        /// Returns the NSwag.Annotations.AzureFunctionsV2 attributes the given method has.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static SwaggerAzureFunctionAttributeCollection GetAttributes(MethodInfo method)
        {
            var collection = new SwaggerAzureFunctionAttributeCollection();
            collection.RequestBodyTypeAttribute = method.GetCustomAttributes()
                .SingleOrDefault(x => x.GetType().Name == "SwaggerRequestBodyTypeAttribute");
            collection.HeaderAttributes = method.GetCustomAttributes()
                .Where(x => x.GetType().Name == "SwaggerRequestHeaderAttribute").ToArray();
            collection.QueryAttributes = method.GetCustomAttributes()
                .Where(x => x.GetType().Name == "SwaggerRequestHeaderAttribute").ToArray();
            collection.UploadFileAttributes = method.GetCustomAttributes()
                .Where(x => x.GetType().Name == "SwaggerFormDataFileAttribute").ToArray();
            collection.FormDataAttributes = method.GetCustomAttributes()
                .Where(x => x.GetType().Name == "SwaggerFormDataAttribute").ToArray();

            return collection;
        }
    }
}
