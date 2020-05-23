using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using fake_data_loader.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;


// USE BOGUS LIbrary for sampledata

namespace fake_data_loader
{
    public class Loader
    {
        IConfiguration config;
        CloudTable wiwTable;

        private List<Model.WhoIsWhoEntity> tags = new List<Model.WhoIsWhoEntity>();
        private List<Model.WhoIsWhoEntity> users = new List<Model.WhoIsWhoEntity>();
        private List<Model.WhoIsWhoEntity> groups = new List<Model.WhoIsWhoEntity>();
        private List<Model.WhoIsWhoEntity> subscriptions = new List<Model.WhoIsWhoEntity>();
        private List<Model.WhoIsWhoEntity> resourceGroups = new List<Model.WhoIsWhoEntity>();
        private List<Model.WhoIsWhoEntity> resources = new List<Model.WhoIsWhoEntity>();
        private List<string> roles = new List<string>();

        public Loader()
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("Settings.json", true, true)
                .Build();
        }

        public async Task Run()
        {
            await CleanTable();

            LoadRBACRoles();
            
            await LoadTags();

            await LoadUsers();
            
            await LoadGroups();

            await LoadSubscriptions();

            await LoadResourceGroups();

            await LoadResources();

            await LoadApplications();

            return;
        }

        private void LoadRBACRoles()
        {
            const string FILENAME_ROLES = "..\\fake-data\\roles.csv";

            using (var rd = new StreamReader(FILENAME_ROLES))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var role = splits[0];
                    
                    roles.Add(role);

                    Console.WriteLine($"Azure Role {role} added successfully");
                }
            }
        }

        private async Task LoadApplications()
        {
            Random rand = new Random();

            const string FILENAME_APPLICATIONS = "..\\fake-data\\applications.csv";

            using (var rd = new StreamReader(FILENAME_APPLICATIONS))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var wiw = new Model.WhoIsWhoEntity(Model.ItemType.Application, splits[0]);
                    wiw.Name = splits[1];
                    wiw.DeepLink = @"https://www.microsoft.com";

                    wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);

                    int randUsers=rand.Next(4);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInApplication,Guid.NewGuid().ToString());
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup.ApplicationId = wiw.RowKey;
                        userGroup.UserId = users[rand.Next(users.Count)].RowKey;
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);
                        Console.WriteLine($"User added to subscription {wiw.RowKey} added successfully");
                    }

                    randUsers=rand.Next(4);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInApplication,Guid.NewGuid().ToString());
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup.ApplicationId = wiw.RowKey;
                        userGroup.GroupId = groups[rand.Next(groups.Count)].RowKey;
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);

                        Console.WriteLine($"Group added to subscription {wiw.RowKey} added successfully");
                    }

                    Console.WriteLine($"Subscription {wiw.RowKey} added successfully");
                }
            }
        }

        private async Task LoadResources()
        {
            Random rand = new Random();

            const string FILENAME_RESOURCES = "..\\fake-data\\resources.csv";

            using (var rd = new StreamReader(FILENAME_RESOURCES))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var wiw = new Model.WhoIsWhoEntity(Model.ItemType.Resource, splits[0]);
                    wiw.Name = splits[1];
                    wiw.DeepLink = @"https://www.microsoft.com";
                    wiw.ResourceGroupId = resourceGroups[rand.Next(resourceGroups.Count)].RowKey;

                    wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                    resources.Add(wiw);

                    int randUsers=rand.Next(5);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInResource,Guid.NewGuid().ToString());
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);
                        userGroup.UserId = users[rand.Next(users.Count)].RowKey;
                        userGroup.ResourceId = wiw.RowKey;
                        Console.WriteLine($"User added to resource {wiw.RowKey} added successfully");
                    }

                    randUsers=rand.Next(5);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInResource,Guid.NewGuid().ToString() );
                        userGroup.GroupId = groups[rand.Next(groups.Count)].RowKey;
                        userGroup.ResourceId = wiw.RowKey;
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);

                        Console.WriteLine($"Group added to resource {wiw.RowKey} added successfully");
                    }

                    Console.WriteLine($"Resource {wiw.RowKey} added successfully");
                }
            }
        }

        private async Task LoadResourceGroups()
        {
            Random rand = new Random();

            const string FILENAME_RESOURCEGROUPS = "..\\fake-data\\resourcegroups.csv";

            using (var rd = new StreamReader(FILENAME_RESOURCEGROUPS))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var wiw = new Model.WhoIsWhoEntity(Model.ItemType.ResourceGroup, splits[0]);
                    wiw.Name = splits[1];
                    wiw.DeepLink = @"https://www.microsoft.com";
                    wiw.SubscriptionId = subscriptions[rand.Next(subscriptions.Count)].RowKey;

                    wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                    resourceGroups.Add(wiw);

                    int randUsers=rand.Next(5);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInResourceGroup, Guid.NewGuid().ToString() );
                        userGroup.UserId = users[rand.Next(users.Count)].RowKey;
                        userGroup.ResourceGroupId = wiw.RowKey;
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);

                        Console.WriteLine($"User added to resourcegroup {wiw.RowKey} added successfully");
                    }

                    randUsers=rand.Next(5);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInResourceGroup,$"{wiw.RowKey}|{groups[rand.Next(groups.Count)].RowKey}" );
                        userGroup.GroupId = groups[rand.Next(groups.Count)].RowKey;
                        userGroup.ResourceGroupId = wiw.RowKey;
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);

                        Console.WriteLine($"Group added to resourcegroup {wiw.RowKey} added successfully");
                    }

                    Console.WriteLine($"Subscription {wiw.RowKey} added successfully");
                }
            }
        }

        private async Task LoadSubscriptions()
        {
            Random rand = new Random();

            const string FILENAME_SUBSCRIPTIONS = "..\\fake-data\\subscriptions.csv";

            using (var rd = new StreamReader(FILENAME_SUBSCRIPTIONS))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var wiw = new Model.WhoIsWhoEntity(Model.ItemType.Subscription, splits[0]);
                    wiw.Name = splits[1];
                    wiw.DeepLink = @"https://www.microsoft.com";

                    wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                    subscriptions.Add(wiw);

                    int randUsers=rand.Next(5);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInSubscription, Guid.NewGuid().ToString() );
                        userGroup.UserId = users[rand.Next(users.Count)].RowKey;
                        userGroup.SubscriptionId = wiw.RowKey;
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);

                        Console.WriteLine($"User added to subscription {wiw.RowKey} added successfully");
                    }

                    randUsers=rand.Next(5);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserGroupInSubscription,$"{wiw.RowKey}|{groups[rand.Next(groups.Count)].RowKey}" );
                        userGroup.GroupId = groups[rand.Next(groups.Count)].RowKey;
                        userGroup.SubscriptionId = wiw.RowKey;
                        userGroup.Name = roles[rand.Next(roles.Count)];
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);

                        Console.WriteLine($"Group added to subscription {wiw.RowKey} added successfully");
                    }

                    Console.WriteLine($"Subscription {wiw.RowKey} added successfully");


                }
            }

        }

        private async Task LoadUsers()
        {
            const string FILENAME_USERS = "..\\fake-data\\users.csv";

            using (var rd = new StreamReader(FILENAME_USERS))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var wiw = new Model.WhoIsWhoEntity(Model.ItemType.User, splits[0]);
                    wiw.Name = splits[1];
                    wiw.Surname = splits[2];
                    wiw.UserType = splits[3];
                    wiw.Mail = splits[4];
                    wiw.Department = splits[5];
                    wiw.DeepLink = @"https://www.microsoft.com";

                    wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);

                    users.Add(wiw);
                    
                    Console.WriteLine($"User {wiw.RowKey} added successfully");
                }
            }
        }

        private async Task LoadTags()
        {
            const string FILENAME_USERS = "..\\fake-data\\tags.csv";

            using (var rd = new StreamReader(FILENAME_USERS))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var wiw = new Model.WhoIsWhoEntity(Model.ItemType.Tag, splits[0]);
                    wiw.Name = splits[1];
                    
                    wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);

                    tags.Add(wiw);
                    
                    Console.WriteLine($"Tag {wiw.RowKey} added successfully");
                }
            }
        }
        private async Task LoadGroups()
        {
            Random rand = new Random();

            const string FILENAME_GROUPS = "..\\fake-data\\groups.csv";

            using (var rd = new StreamReader(FILENAME_GROUPS))
            {
                rd.ReadLine(); // skip first line

                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(';');
                    var wiw = new Model.WhoIsWhoEntity(Model.ItemType.Group, splits[0]);
                    wiw.Name = splits[1];
                    wiw.GroupType = splits[2];
                    wiw.DeepLink = @"https://www.microsoft.com";
                    
                    wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);

                    groups.Add(wiw);

                    int randUsers=rand.Next(10);
                    for (int i=0; i<randUsers; i++)
                    {
                        var userGroup= new WhoIsWhoEntity(Model.ItemType.UserInGroup,$"{wiw.RowKey}-{users[rand.Next(users.Count)].RowKey}" );
                        userGroup.UserId = users[rand.Next(users.Count)].RowKey;
                        userGroup.GroupId = wiw.RowKey; 
                        userGroup = await InsertOrMergeEntityAsync( wiwTable, userGroup);

                        Console.WriteLine($"User added to {wiw.RowKey} added successfully");
                    }

                    Console.WriteLine($"Group {wiw.RowKey} added successfully");
                }
            }
        }

        private async Task CleanTable()
        {
            Console.WriteLine("creating table begin\n");

            string tableName = "WhoIsWhoBase";
            wiwTable = await CreateTableAsync(tableName);

            Console.WriteLine("creating table done\n");
        }

        private async Task<CloudTable> CreateTableAsync(string tableName)
        {
            string storageConnectionString = config["StorageConnectionString"];

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
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
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        private async Task<WhoIsWhoEntity> InsertOrMergeEntityAsync(CloudTable table, WhoIsWhoEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                WhoIsWhoEntity insertedCustomer = result.Result as WhoIsWhoEntity;

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

    }
}