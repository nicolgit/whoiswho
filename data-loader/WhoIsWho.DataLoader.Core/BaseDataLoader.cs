using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Core
{
    public abstract class BaseDataLoader : TableStoragePersistance, IBaseDataLoader
    {
        public const string TableSourceSuffix = "Source";
        public const string StorageConnectionKey = "StorageConnectionString";

        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private CloudTable CurrentTable;

        public BaseDataLoader(IConfiguration configuration, ILogger logger) : base(logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public BaseDataLoader(IConfiguration configuration, ILogger logger, string LoaderIdentifier) : this(configuration, logger)
        {
            this.LoaderIdentifier = LoaderIdentifier;
        }

        public BaseDataLoader(IConfiguration configuration, ILogger logger, string LoaderIdentifier, string TableSuffix, bool recreateStructure) : this(configuration, logger, LoaderIdentifier)
        {
            this.SuffixToUse = TableSuffix;
            this.RecreateStructure = recreateStructure;
        }

        public string LoaderIdentifier { get; }

        public string SuffixToUse { get; set; } = TableSourceSuffix;

        public bool RecreateStructure { get; set; } = true;

        public abstract Task LoadData();

        public async Task EnsureTableExists()
        {
            if (CurrentTable == null)
                CurrentTable = await CreateTableAsync(FormatTableName(LoaderIdentifier, SuffixToUse));
        }

        public static string FormatTableName(string tableIdentified, string suffixToUse)
        {
            return tableIdentified + suffixToUse;
        }

        private async Task<CloudTable> CreateTableAsync(string tableName)
        {
            string storageConnectionString = configuration[StorageConnectionKey];
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            if (RecreateStructure)
            {
                await table.DeleteIfExistsAsync();
            }
            bool created = false;
            while (!created)
            {
                try
                {
                    if (await table.CreateIfNotExistsAsync())
                        logger.LogInformation("Created Table named: {0}", tableName);
                    else
                        logger.LogInformation("Table {0} already exists", tableName);
                    created = true;
                }
                catch (Exception e)
                {
                    int retry = 4;
                    logger.LogInformation($"CreateTableAsync ERROR: {e.Message} - retry in {retry} sec");
                    await Task.Delay(1000 * retry);
                }
            }
            return table;
        }



        public async Task<WhoIsWhoEntity> InsertOrMergeEntityAsync(WhoIsWhoEntity entity)
        {
            await EnsureTableExists();
            if (entity == null) throw new ArgumentNullException("entity");
            try
            {
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
                TableResult result = await CurrentTable.ExecuteAsync(insertOrMergeOperation);
                WhoIsWhoEntity insertedCustomer = result.Result as WhoIsWhoEntity;
                if (result.RequestCharge.HasValue)
                {
                    logger.LogInformation("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }
                return insertedCustomer;
            }
            catch (StorageException e)
            {
                logger.LogError(e.Message);
                throw;
            }
        }

        public async Task PartialUpdateBatchAsync(IList<(string partitonKey, string rowKey)> itemIDs, Dictionary<string, EntityProperty> properties)
        {
            var batchs = itemIDs.Select(e =>
              {
                  var entity = new DynamicTableEntity(e.partitonKey, e.rowKey);
                  entity.ETag = "*";
                  foreach (var item in properties)
                  {
                      entity.Properties.Add(item.Key, item.Value);
                  }
                  return TableOperation.Merge(entity);
              }).ToList();

            await ExecuteBatchOperations(batchs);
        }

        private async Task<List<TableBatchResult>> ExecuteBatchOperations(List<TableOperation> ops)
        {
            List<TableBatchResult> res = new List<TableBatchResult>();
            int off = 0;
            while (off < ops.Count)
            {
                // Batch size.
                int len = Math.Min(100, ops.Count - off);
                while (true)
                {
                    var batch = new TableBatchOperation();
                    for (int i = 0; i < len; i++) batch.Add(ops[off + i]);

                    try
                    {
                        res.Add(await CurrentTable.ExecuteBatchAsync(batch));
                        break;
                    }
                    catch (StorageException se)
                    {
                        if (se.RequestInformation?.HttpStatusCode == (int)HttpStatusCode.RequestEntityTooLarge)
                        {
                            len = len * 4000000 / 5000000; //should get EgressBytes but is not present anymore in the RequestInformation prop
                        }
                        else throw;
                    }
                }

                off += len;
            }

            return res;
        }

    }
}