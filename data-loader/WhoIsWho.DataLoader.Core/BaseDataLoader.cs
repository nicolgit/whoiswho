using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Core
{
    public abstract class BaseDataLoader : IBaseDataLoader
    {
        public const string TableSuffix = "Source";
        public const string StorageConnectionKey = "StorageConnectionString";

        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private CloudTable CurrentTable;

        public BaseDataLoader(IConfiguration configuration, ILogger logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public abstract string LoaderIdentifier { get; }
        public abstract Task LoadData();

        public async Task EnsureTableExists()
        {
            if (CurrentTable == null)
                CurrentTable = await CreateTableAsync(LoaderIdentifier + TableSuffix);
        }

        private async Task<CloudTable> CreateTableAsync(string tableName)
        {
            string storageConnectionString = configuration[StorageConnectionKey];
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            await table.DeleteIfExistsAsync();
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

        private CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                logger.LogInformation("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                logger.LogInformation("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid - then restart the application.");
                throw;
            }

            return storageAccount;
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

    }
}