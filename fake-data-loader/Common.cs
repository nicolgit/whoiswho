using System;

namespace fake_data_loader
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;

    public class Common
    {
        IConfiguration config;

        public Common()
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("Settings.json", true, true)
                .Build();
        }
        

        public static async Task<CloudTable> CreateTableAsync(CloudTableClient tableClient, string tableName)
        {
            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            try
            {
                if (await table.CreateIfNotExistsAsync())
                {
                    Console.WriteLine("Created Table named: {0}", tableName);
                }
                else
                {
                    Console.WriteLine("Table {0} already exists", tableName);
                }
            }
            catch (StorageException)
            {
                Console.WriteLine(
                    "If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
            return table;
        }

        
    }
}