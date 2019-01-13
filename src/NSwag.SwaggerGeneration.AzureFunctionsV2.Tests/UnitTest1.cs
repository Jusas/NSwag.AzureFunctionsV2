using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp;
using Xunit;

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task When_function_app_has_route_params_they_are_generated_properly()
        {
            var generator = new AzureFunctionsV2ToSwaggerGenerator(new AzureFunctionsV2ToSwaggerGeneratorSettings());

            var document = await generator.GenerateForAzureFunctionClassAsync(typeof(Functions1));

            var json = document.ToJson();
        }
        
    }
}
