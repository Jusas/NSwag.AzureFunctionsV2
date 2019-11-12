using System;
using System.Collections.Generic;
using System.Text;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp
{

    // Requests

    public class RequestBodyModelWithPrimitives
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }

    public class RequestBodyModelWithComplexType
    {
        public string Primitive { get; set; }
        public RequestBodyModelWithPrimitives Complex { get; set; }
    }

    // Responses

    public class ResponseModelWithPrimitives
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }


}
