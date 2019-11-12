using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp;

[assembly: WebJobsStartup(typeof(Startup), "MyStartup")]

namespace NSwag.SwaggerGeneration.AzureFunctionsV2.Tests.TestFunctionApp
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
        }
    }
}
