using NSwag.SwaggerGeneration.AzureFunctionsV2.Processors;
using NSwag.SwaggerGeneration.Processors;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2
{
    public class AzureFunctionsV2ToSwaggerGeneratorSettings : SwaggerGeneratorSettings
    {
        /// <summary>Initializes a new instance of the <see cref="AzureFunctionsV2ToSwaggerGeneratorSettings"/> class.</summary>
        public AzureFunctionsV2ToSwaggerGeneratorSettings()
        {
            OperationProcessors.Insert(0, new ApiVersionProcessor());
            OperationProcessors.Insert(3, new OperationParameterProcessor(this));
            OperationProcessors.Insert(3, new OperationResponseProcessor(this));
            OperationProcessors.Add(new OperationSecurityScopeProcessor());
        }

        /// <summary>Gets or sets a value indicating whether to add path parameters which are missing in the action method.</summary>
        public bool AddMissingPathParameters { get; set; }
    }
}