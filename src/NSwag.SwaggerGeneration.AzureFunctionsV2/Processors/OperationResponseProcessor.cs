using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Processors
{
    public class OperationResponseProcessor : OperationResponseProcessorBase, IOperationProcessor
    {

        public OperationResponseProcessor(AzureFunctionsV2ToSwaggerGeneratorSettings settings) 
            : base(settings)
        {
        }

        public async Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var responseTypeAttributes = context.MethodInfo.GetCustomAttributes()
                .Where(a => a.GetType().Name == "ResponseTypeAttribute" ||
                            a.GetType().Name == "SwaggerResponseAttribute")
                .ToList();

            var producesResponseTypeAttributes = context.MethodInfo.GetCustomAttributes()
                .Where(a => a.GetType().Name == "ProducesResponseTypeAttribute" ||
                            a.GetType().Name == "ProducesAttribute")
                .ToList();

            var parameter = context.MethodInfo.ReturnParameter;
            var attributes = responseTypeAttributes.Concat(producesResponseTypeAttributes);

            await ProcessResponseTypeAttributes(context, parameter, attributes);

            return true;
        }

        protected override string GetVoidResponseStatusCode()
        {
            return "204";
        }
    }
}