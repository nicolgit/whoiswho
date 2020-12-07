using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Azure.Loaders;

namespace WhoIsWho.DataLoader.Azure
{
    public class AADLoader
    {
        private readonly AADDataLoader aadLoader;

        public AADLoader(AADDataLoader aadLoader)
        {
            this.aadLoader = aadLoader;
        }

        [FunctionName("AADLoader")]
        public async Task Run([TimerTrigger("%AADLoaderCronTimer%")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Execution started at: {DateTime.UtcNow}");

            await aadLoader.ExecutLoadDataAsync();

            log.LogInformation($"Execution ended at: {DateTime.UtcNow}");
        }
    }
}
