using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Sync.Services.Abstract;

namespace WhoIsWho.DataLoader.Sync
{
    public class WhoIsWhoSync
    {

        private readonly IWhoIsWhoDataSyncronizer dataSyncronizer;

        public WhoIsWhoSync(IWhoIsWhoDataSyncronizer dataSyncronizer)
        {
            this.dataSyncronizer = dataSyncronizer;
        }

        [FunctionName("WhoIsWhoSync")]
        public async Task Run([TimerTrigger("%WhoIsWhoSyncTimer%", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Execution started at: {DateTime.Now}");

            await dataSyncronizer.ExecuteSynronizationAsync();

            log.LogInformation($"Execution ended at: {DateTime.Now}");
        }
    }
}
