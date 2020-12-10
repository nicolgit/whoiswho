using AutoMapper;
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

        public WhoIsWhoDataSyncronizer(IConfiguration configuration, ILogger<WhoIsWhoDataSyncronizer> logger, IWhoIsWhoDataReader reader, IMapper mapper) : base(configuration, logger, SyncSuffix, false)
        {
            _logger = logger;
            _reader = reader;
            _mapper = mapper;
        }

        public async Task ExecuteDataSyncronizationAsync(string loaderIdentifier)
        {
            this.LoaderIdentifier = loaderIdentifier;
            await LoadDataAsync();
        }

        public override async Task LoadDataAsync()
        {
            await this.EnsureTableExistsAsync();
            var alreadyExistentID = await _reader.ReadDataAsync(FormatTableName(LoaderIdentifier, SyncSuffix), new TableQuery<TableEntity>().Select(new[] { "RowKey" })).Select(x => (x.PartitionKey, x.RowKey)).ToListAsync();

            _logger.LogDebug($"Read {alreadyExistentID.Count} existent items to sync for the loader {LoaderIdentifier}");

            await foreach (var item in _reader.ReadDataAsync(FormatTableName(LoaderIdentifier, TableSourceSuffix), new TableQuery<WhoIsWhoEntity>()))
            {
                alreadyExistentID.Remove((item.PartitionKey, item.RowKey));
                WhoIsWhoSyncedEntity currentEntity = _mapper.Map<WhoIsWhoSyncedEntity>(item);
                await this.InsertOrMergeEntityAsync(currentEntity);
            }

            _logger.LogDebug($"Detected {alreadyExistentID.Count} items to be deleted for the loader {LoaderIdentifier}");

            var groupForPartition = alreadyExistentID.GroupBy(x => x.PartitionKey);
            foreach (var group in groupForPartition)
            {
                await this.PartialUpdateBatchAsync(group.ToList(), new Dictionary<string, EntityProperty> { { nameof(WhoIsWhoSyncedEntity.IsDeleted), new EntityProperty(true) } });
            }
        }
    }
}