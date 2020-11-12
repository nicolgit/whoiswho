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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            int number = 0;
            bool isParsable = Int32.TryParse(req.Query["number"], out number);

            if (number > 0)
            {
                log.LogInformation($"Execution started at: {DateTime.Now}");
                fakeLoader.Number= number;
                await fakeLoader.LoadData();
                log.LogInformation($"Execution ended at: {DateTime.Now}");

                return new OkObjectResult($"{number} objects created successfully");
            }
            else
            {
                return new BadRequestObjectResult("Please pass a number on the query string to create fake items");
            }
        }
    }
}
