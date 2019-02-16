using NSwag.SwaggerGeneration.AzureFunctionsV2.Processors;
using NSwag.SwaggerGeneration.Processors;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2
{
    /// <summary>
    /// The SwaggerGenerator settings.
    /// <para>
    /// The default settings set up <see cref="OperationParameterProcessor"/> and <see cref="OperationResponseProcessor"/>.
    /// Security processors must be added manually matching your security usage (Basic, OAuth2, ApiKey...).
    /// </para>
    /// </summary>
    public class AzureFunctionsV2ToSwaggerGeneratorSettings : SwaggerGeneratorSettings
    {
        /// <summary>Initializes a new instance of the <see cref="AzureFunctionsV2ToSwaggerGeneratorSettings"/> class.</summary>
        public AzureFunctionsV2ToSwaggerGeneratorSettings()
        {
            OperationProcessors.Insert(0, new ApiVersionProcessor());
            OperationProcessors.Insert(3, new OperationParameterProcessor(this));
            OperationProcessors.Insert(3, new OperationResponseProcessor(this));
        }

        /// <summary>Gets or sets a value indicating whether to add path parameters which are missing in the action method.</summary>
        public bool AddMissingPathParameters { get; set; }
    }
}