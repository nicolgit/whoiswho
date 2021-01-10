using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace WhoIsWho.DataLoader.Core
{
    public class TableStoragePersistance
    {
        public const string StorageDefaultConnectionKey = "StorageConnectionString";
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        public TableStoragePersistance(IConfiguration configuration, ILogger logger)
        {
            this.logger = logger;
            this.configuration = configuration;
        }


        public string GetConnectionString(string loaderIdentifier)
        {
            var loaderIdentifierConnectionString = configuration[$"{StorageDefaultConnectionKey}:{loaderIdentifier}"];
            if (!string.IsNullOrWhiteSpace(loaderIdentifierConnectionString)) return loaderIdentifierConnectionString;
            return configuration[StorageDefaultConnectionKey];
        }

        public CloudStorageAccount InitializeStorageAccountFromConnectionString(string storageConnectionString)
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
    }
}
