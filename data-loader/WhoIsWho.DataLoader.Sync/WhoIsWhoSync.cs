using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Sync.Services;

namespace WhoIsWho.DataLoader.Sync
{
    public class WhoIsWhoSync
    {

        private readonly IWhoIsWhoDataSyncronizer _dataSyncronizer;

        public WhoIsWhoSync(IWhoIsWhoDataSyncronizer syncronizer)
        {
            this._dataSyncronizer = syncronizer;
        }

        [FunctionName("WhoIsWhoSync")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req, ILogger log)
        {
            var loader = req.Query["loader"];
            log.LogInformation($"Execution requested at: {DateTime.Now}");

            //This task will not be monitored except that for logging information: consider to use Durable Function in order to implement the 202/Status pattern
            var task = new TaskFactory().StartNew(async () =>
                {
                    try
                    {
                        await _dataSyncronizer.ExecuteDataSyncronizationAsync(loader);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, ex.Message);
                    }
                    
                    log.LogInformation($"Execution ended at: {DateTime.Now}");
                }
            );

            log.LogInformation($"Syncronization accepted at: {DateTime.Now}");

            return new AcceptedResult();
        }
    }
}
