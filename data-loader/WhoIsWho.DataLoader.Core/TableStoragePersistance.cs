using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhoIsWho.DataLoader.Core
{
    public class TableStoragePersistance
    {

        private readonly ILogger logger;

        public TableStoragePersistance(ILogger logger)
        {
            this.logger = logger;
        }

        public CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
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
