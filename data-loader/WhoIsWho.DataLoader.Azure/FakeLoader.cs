using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Azure.Loaders;
using Microsoft.AspNetCore.Http;

namespace WhoIsWho.DataLoader.Azure
{
    public class FakeLoader
    {
        private readonly FakeDataLoader fakeLoader;

        public FakeLoader(FakeDataLoader fakeLoader)
        {
            this.fakeLoader = fakeLoader;
        }

        [FunctionName("FakeLoader")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {           
            log.LogInformation($"Execution started at: {DateTime.Now}");
            await fakeLoader.LoadData();
            log.LogInformation($"Execution ended at: {DateTime.Now}");

            return new OkObjectResult($"fake objects created successfully");
        }
    }
}
