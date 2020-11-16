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
    public class WhoIsWhoDataSyncronizer : BaseDataLoader
    {
        private readonly ILogger _logger;
        private readonly IWhoIsWhoDataReader _reader;
        private const string SyncSuffix = "Sync";
        private readonly string _sourceTable;
        private readonly IMapper _mapper;

        public WhoIsWhoDataSyncronizer(IConfiguration configuration, ILogger<WhoIsWhoDataSyncronizer> logger, IWhoIsWhoDataReader reader, IMapper mapper, string sourceTable) : base(configuration, logger, sourceTable, SyncSuffix, false)
        {
            _logger = logger;
            _reader = reader;
            _sourceTable = sourceTable;
            _mapper = mapper;
        }

        public override async Task LoadData()
        {
            var alreadyExistentID = await _reader.ReadDataAsync(BaseDataLoader.FormatTableName(_sourceTable, SyncSuffix), new TableQuery<TableEntity>().Select(new[] { "RowKey" })).Select(x => (x.PartitionKey, x.RowKey)).ToListAsync();


            await foreach (var item in _reader.ReadDataAsync(BaseDataLoader.FormatTableName(_sourceTable, BaseDataLoader.TableSourceSuffix), new TableQuery<WhoIsWhoEntity>()))
            {
                alreadyExistentID.Remove((item.PartitionKey, item.RowKey));
                WhoIsWhoSyncedEntity currentEnttity = _mapper.Map<WhoIsWhoSyncedEntity>(item);
                await this.InsertOrMergeEntityAsync(currentEnttity);
            }

            await this.PartialUpdateBatchAsync(alreadyExistentID, new Dictionary<string, EntityProperty> { { nameof(WhoIsWhoSyncedEntity.IsDeleted), new EntityProperty(true) } });
        }
    }
}
