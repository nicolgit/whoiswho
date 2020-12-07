using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Azure.Loaders;

namespace WhoIsWho.DataLoader.Azure
{
    public class ARMLoader
    {
        private readonly ARMDataLoader armLoader;

        public ARMLoader(ARMDataLoader armLoader)
        {
            this.armLoader = armLoader;
        }

        [FunctionName("ARMLoader")]
        public async Task Run([TimerTrigger("%ARMLoaderCronTimer%")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Execution started at: {DateTime.Now}");

            await armLoader.ExecutLoadDataAsync();

            log.LogInformation($"Execution ended at: {DateTime.Now}");
        }
    }
}
