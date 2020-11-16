using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Sync.Services;

namespace WhoIsWho.DataLoader.Sync
{
    public class WhoIsWhoSync
    {

        private readonly IEnumerable<WhoIsWhoDataSyncronizer> _dataSyncronizers;
        private readonly IWhoIsWhoDataReader _dataReader;

        public WhoIsWhoSync(IEnumerable<WhoIsWhoDataSyncronizer> syncronizers, IWhoIsWhoDataReader dataReader)
        {
            this._dataSyncronizers = syncronizers;
            this._dataReader = dataReader;
        }

        [FunctionName("WhoIsWhoSync")]
        public async Task Run([TimerTrigger("%WhoIsWhoSyncTimer%", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Execution started at: {DateTime.Now}");

            await Task.WhenAll(_dataSyncronizers.Select(x => x.LoadData()));

            log.LogInformation($"Execution ended at: {DateTime.Now}");
        }
    }
}
