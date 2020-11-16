using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Core
{
    public class WhoIsWhoDataReader : TableStoragePersistance, IWhoIsWhoDataReader
    {
        public const string StorageConnectionKey = "StorageConnectionString";
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private CloudTable CurrentTable;

        public WhoIsWhoDataReader(IConfiguration configuration, ILogger logger) : base(logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }


        public async IAsyncEnumerable<T> ReadDataAsync<T>(string tableName, TableQuery<T> query) where T : ITableEntity, new()
        {
            string storageConnectionString = configuration[StorageConnectionKey];
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            this.CurrentTable = tableClient.GetTableReference(tableName);
            TableContinuationToken token = null;
            do
            {

                TableQuerySegment<T> seg = await CurrentTable.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                foreach (var currentElement in seg)
                {
                    yield return currentElement;
                }
            } while (token != null);

        }

    }
}
