using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bogus;
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
        private List<string> roles;
        private List<string> resourceTypes;

        public Loader()
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("Settings.json", true, true)
                .Build();
        }

        public async Task Run()
        {
            Randomizer.Seed = new Random(1972);
            
            await CleanTable();

            LoadRBACRoles();

            LoadResourceTypes();            
            
            await LoadTags();

            await LoadUsers();
            
            await LoadGroups();
            
            await LoadSubscriptions();
            
            await LoadResourceGroups();

            await LoadAzureResources();

            await LoadApplications();
            
            return;
        }

        private void LoadRBACRoles()
        {
            var localRoles = new[] { "Reader", "Contributor", "Owner", "User Access Administrator"};
            
            roles = new List<string>();

            foreach (var role in localRoles)
            {
                roles.Add(role);
            }

            Console.WriteLine($"Azure Roles created successfully");
        }

         private void LoadResourceTypes()
        {
            var local = new[] { "AppService", "Virtual Machine", "SQL Azure", "Search Service", "BOT Service", "Cosmos DB","Data Lake"};
            
            resourceTypes = new List<string>();

            foreach (var role in local)
            {
                resourceTypes.Add(role);
            }

            Console.WriteLine($"Azure resource Types created successfully");
        }

        private async Task LoadApplications()
        {
            var rand = new Bogus.Randomizer();

            var fakeApplication= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.Application}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"res {f.Commerce.Product()} {Guid.NewGuid()}")
                .RuleFor(o => o.Type, f => $"{Model.ItemType.Application}")
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath());

            var fakeUserInApplication = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInApplication}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.UserId, f => f.PickRandom(users).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInApplication}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            var fakeGroupInApplication = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInApplication}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.GroupId, f => f.PickRandom(groups).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInApplication}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            int max = 50;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeApplication.Generate();
                wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                resourceGroups.Add(wiw);

                Console.WriteLine($"Application added successfully {wiw.ToString()}");

                int usersInApplication = rand.Int(3,5);
                for (int j=0; j<usersInApplication; j++)
                {
                    var uig = fakeUserInApplication.Generate();
                    uig.ApplicationId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"User In Application {uig.ToString()}");
                }

                int groupsInApplication = rand.Int(3,5);
                for (int j=0; j<groupsInApplication; j++)
                {
                    var uig = fakeUserInApplication.Generate();
                    uig.ApplicationId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"Group In Application {uig.ToString()}");
                }
            }
        }

        private async Task LoadAzureResources()
        {
            var rand = new Bogus.Randomizer();

            var fakeResource= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.Resource}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"res {f.Commerce.ProductName()} {f.UniqueIndex}")
                .RuleFor(o => o.Type, f => $"{Model.ItemType.Resource}")
                .RuleFor(o => o.ResourceType, f =>  f.PickRandom(resourceTypes))
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath());

            var fakeUserInResource = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInResource}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.UserId, f => f.PickRandom(users).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInResource}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            var fakeGroupInResource = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInResource}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.GroupId, f => f.PickRandom(groups).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInSubscription}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            int max = 50;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeResource.Generate();
                wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                resources.Add(wiw);

                Console.WriteLine($"ResourceGroup added successfully {wiw.ToString()}");

                int usersInResource = rand.Int(3,5);
                for (int j=0; j<usersInResource; j++)
                {
                    var uig = fakeUserInResource.Generate();
                    uig.ResourceId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"User In Resource {uig.ToString()}");
                }

                int groupsInResource = rand.Int(3,5);
                for (int j=0; j<groupsInResource; j++)
                {
                    var uig = fakeGroupInResource.Generate();
                    uig.ResourceId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"Group In Resource {uig.ToString()}");
                }
            }
        }

        private async Task LoadResourceGroups()
        {
            var rand = new Bogus.Randomizer();

            var fakeResourceGroups= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.ResourceGroup}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"rg {f.Commerce.Color()} {f.UniqueIndex}")
                .RuleFor(o => o.Type, f => $"{Model.ItemType.ResourceGroup}")
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath());

            var fakeUserInResourceGroup = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInResourceGroup}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.UserId, f => f.PickRandom(users).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInResourceGroup}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            var fakeGroupInSubscription = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInResourceGroup}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.GroupId, f => f.PickRandom(groups).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInResourceGroup}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            int max = 50;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeResourceGroups.Generate();
                wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                resourceGroups.Add(wiw);

                Console.WriteLine($"ResourceGroup added successfully {wiw.ToString()}");

                int usersInResourceGroup = rand.Int(3,5);
                for (int j=0; j<usersInResourceGroup; j++)
                {
                    var uig = fakeUserInResourceGroup.Generate();
                    uig.ResourceGroupId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"User In ResourceGroup {uig.ToString()}");
                }

                int groupsInResourceGroup = rand.Int(3,5);
                for (int j=0; j<groupsInResourceGroup; j++)
                {
                    var uig = fakeResourceGroups.Generate();
                    uig.ResourceGroupId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"Group In ResourceGroup {uig.ToString()}");
                }
            }
        }

        private async Task LoadSubscriptions()
        {
            var rand = new Bogus.Randomizer();

            var fakeSubscriptions= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.Subscription}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"subscription {f.Commerce.ProductMaterial()} {f.UniqueIndex}")
                .RuleFor(o => o.Type, f => $"{Model.ItemType.Subscription}")
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath());

            var fakeUserInSubscription = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInSubscription}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.UserId, f => f.PickRandom(users).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInSubscription}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            var fakeGroupInSubscription = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserGroupInSubscription}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.GroupId, f => f.PickRandom(groups).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserGroupInSubscription}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            int max = 50;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeSubscriptions.Generate();
                wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                subscriptions.Add(wiw);

                Console.WriteLine($"Subscription added successfully {wiw.ToString()}");

                int usersInSubscriptions = rand.Int(3,5);
                for (int j=0; j<usersInSubscriptions; j++)
                {
                    var uig = fakeUserInSubscription.Generate();
                    uig.SubscriptionId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"User In Subscription {uig.ToString()}");
                }

                int groupsInSubscriptions = rand.Int(3,5);
                for (int j=0; j<groupsInSubscriptions; j++)
                {
                    var uig = fakeGroupInSubscription.Generate();
                    uig.GroupId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"Group In Subscription {uig.ToString()}");
                }
            }
        }

        private async Task LoadUsers()
        {
            var usertypes = new[] { "AD user", "Guest" };

            var rand = new Bogus.Randomizer();

            var fakeUsers= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.User}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => f.Name.FirstName())
                .RuleFor(o => o.Type, f => $"{Model.ItemType.User}")
                .RuleFor(o => o.Surname, f => f.Name.LastName())
                .RuleFor(o => o.UserType, f => f.PickRandom(usertypes))
                .RuleFor(o => o.Mail, f => f.Internet.Email())
                .RuleFor(o => o.Department, f=>f.Commerce.Department())
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath())
                .RuleFor(o=> o.ImgUrl, f=>f.Image.PicsumUrl());

               
            int max = 150;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeUsers.Generate();

                wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                
                users.Add(wiw);
                Console.WriteLine($"User added successfully {wiw.ToString()}");
            }
        }

        private async Task LoadTags()
        {
            var rand = new Bogus.Randomizer();

            var fakeTags = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.Tag}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"CLID:{rand.Replace("##-####-####")}")
                .RuleFor(o => o.Type, f => $"{Model.ItemType.Tag}");
            
            int max = 50;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeTags.Generate();

                wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);

                tags.Add(wiw);
                
                Console.WriteLine($"Tag added successfully {wiw.ToString()}");
            }
        }
        private async Task LoadGroups()
        {
            var groupType = new[] { "Mail Enabled", "Security" };

            var rand = new Bogus.Randomizer();

            var fakeGroups= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.Group}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"Group {f.Commerce.ProductName()} {f.UniqueIndex}")
                .RuleFor(o => o.Type, f => $"{Model.ItemType.Group}")
                .RuleFor(o => o.GroupType, f => f.PickRandom(groupType))
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath())
                .RuleFor(o=> o.ImgUrl, f=>f.Image.PicsumUrl());

            var fakeUserInGroup = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, $"{Model.ItemType.UserInGroup}{rand.Replace("-##")}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.UserId, f => f.PickRandom(users).RowKey)
                .RuleFor(o => o.Type, f => $"{Model.ItemType.UserInGroup}");


            int max = 50;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeGroups.Generate();
                wiw = await InsertOrMergeEntityAsync( wiwTable, wiw);
                groups.Add(wiw);

                Console.WriteLine($"User added successfully {wiw.ToString()}");

                int usersInGroup = rand.Int(3,5);
                for (int j=0; j<usersInGroup; j++)
                {
                    var uig = fakeUserInGroup.Generate();
                    uig.GroupId = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync( wiwTable, uig);
                    Console.WriteLine($"User In Group {uig.ToString()}");
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