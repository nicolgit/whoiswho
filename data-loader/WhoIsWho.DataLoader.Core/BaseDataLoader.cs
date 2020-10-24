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
						Console.WriteLine("Created Table named: {0}", tableName);
					else
						Console.WriteLine("Table {0} already exists", tableName);
					created = true;
				}
				catch (Exception e)
				{
					int retry = 2;
					Console.WriteLine($"ERROR: {e.Message}");
					Console.WriteLine($"retry in {retry} sec");
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

		public async Task<IEnumerable<WhoIsWhoEntity>> InsertOrMergeEntitiesBatchAsync(IEnumerable<WhoIsWhoEntity> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));
			if (!entities.Any())
				return entities;
			var firstPk = entities.First().PartitionKey;
			if (!entities.All(e => e.PartitionKey == firstPk))
				throw new InvalidOperationException("All the entities must belong to the same PartitionKey.");

			await EnsureTableExists();


			List<TableOperation> ops = entities.Select(e => TableOperation.InsertOrMerge(e)).ToList();
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
			return res.SelectMany(r => r.Select(rr => rr.Result as WhoIsWhoEntity));
		}

	}
}