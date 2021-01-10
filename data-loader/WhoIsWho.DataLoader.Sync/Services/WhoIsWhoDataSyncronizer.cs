using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Sync.Services
{
    public class WhoIsWhoDataSyncronizer : BaseDataLoader, IWhoIsWhoDataSyncronizer
    {
        private readonly ILogger _logger;
        private readonly IWhoIsWhoDataReader _reader;
        private const string SyncSuffix = "Sync";
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetryClient;

        public WhoIsWhoDataSyncronizer(IConfiguration configuration, ILogger<WhoIsWhoDataSyncronizer> logger, IWhoIsWhoDataReader reader, IMapper mapper, TelemetryClient telemetryClient) : base(configuration, logger, SyncSuffix, false)
        {
            _logger = logger;
            _reader = reader;
            _mapper = mapper;
            _telemetryClient = telemetryClient;
        }

        public async Task ExecuteDataSyncronizationAsync(string loaderIdentifier)
        {
            this.LoaderIdentifier = loaderIdentifier;
            await LoadDataAsync();
        }

        public override async Task LoadDataAsync()
        {
            long countUpdated = 0;
            long countAdded = 0;
            await this.EnsureTableExistsAsync();
            var alreadyExistentID = await _reader.ReadDataAsync(LoaderIdentifier, FormatTableName(LoaderIdentifier, SyncSuffix), new TableQuery<TableEntity>().Select(new[] { "RowKey" })).Select(x => (x.PartitionKey, x.RowKey)).ToListAsync();

            _logger.LogInformation($"Read {alreadyExistentID.Count} existent items to sync for the loader {LoaderIdentifier}");

            await foreach (var item in _reader.ReadDataAsync(LoaderIdentifier, FormatTableName(LoaderIdentifier, TableSourceSuffix), new TableQuery<WhoIsWhoEntity>()))
            {
                var alreadyExistent = alreadyExistentID.Remove((item.PartitionKey, item.RowKey));
                WhoIsWhoSyncedEntity currentEntity = _mapper.Map<WhoIsWhoSyncedEntity>(item);
                var entity = await this.InsertOrMergeEntityAsync(currentEntity);

                if (alreadyExistent)
                    countUpdated++;
                else
                    countAdded++;

                _logger.LogDebug($"Item with PartitionKey:{item.PartitionKey} and RowKey:{item.RowKey} {(alreadyExistent ? "updated" : "added")} successfully");
            }

            _telemetryClient.TrackMetric($"{LoaderIdentifier}Added", countAdded);
            _telemetryClient.TrackMetric($"{LoaderIdentifier}Updated", countUpdated);

            _logger.LogInformation($"Detected {alreadyExistentID.Count} items to be deleted for the loader {LoaderIdentifier}");

            var groupForPartition = alreadyExistentID.GroupBy(x => x.PartitionKey);
            foreach (var group in groupForPartition)
            {
                await this.PartialUpdateBatchAsync(group.ToList(), new Dictionary<string, EntityProperty> { { nameof(WhoIsWhoSyncedEntity.IsDeleted), new EntityProperty(true) } });

                foreach (var item in group)
                {
                    _logger.LogDebug($"Item with PartitionKey:{item.PartitionKey} and RowKey:{item.RowKey} deleted successfully");
                }
            }
            _telemetryClient.TrackMetric($"{LoaderIdentifier}Deleted", groupForPartition.Sum(x => x.Count()));
        }
    }
}