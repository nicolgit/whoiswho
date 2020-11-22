using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Azure.Models;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Models;
using Bogus;

namespace WhoIsWho.DataLoader.Azure.Loaders
{
    public class FakeDataLoader : BaseDataLoader
    {
        private const int QUANTITY = 5;
        private const int QUANTITY_USERS = 10;
        
        private readonly ILogger logger;

        private List<WhoIsWhoEntity> tags = new List<WhoIsWhoEntity>();
        private List<WhoIsWhoEntity> users = new List<WhoIsWhoEntity>();
        private List<WhoIsWhoEntity> groups = new List<WhoIsWhoEntity>();
        private List<WhoIsWhoEntity> subscriptions = new List<WhoIsWhoEntity>();
        private List<WhoIsWhoEntity> resourceGroups = new List<WhoIsWhoEntity>();
        private List<WhoIsWhoEntity> resources = new List<WhoIsWhoEntity>();
        private List<string> roles;
        private List<string> resourceTypes;

        public override string LoaderIdentifier => nameof(FakeDataLoader);
        
        public FakeDataLoader(IConfiguration configuration, ILogger<ARMDataLoader> logger) : base(configuration, logger)
        {
            this.logger = logger;
        }
        
        public override async Task LoadData()
        {
            Randomizer.Seed = new Random(1972);

            await base.EnsureTableExists();

            LoadRBACRoles();
            LoadResourceTypes();
            await LoadUsersAsync();
            await LoadTags();
            await LoadGroups();
            await LoadSubscriptions();
        }

        private void LoadRBACRoles()
        {
            var localRoles = new[] { "Reader", "Contributor", "Owner", "User Access Administrator"};
            
            roles = new List<string>();

            foreach (var role in localRoles)
            {
                roles.Add(role);
            }

            logger.LogInformation($"Azure Roles created successfully");
        }

        private void LoadResourceTypes()
        {
            var local = new[] { "AppService", "Virtual Machine", "SQL Azure", "Search Service", "BOT Service", "Cosmos DB","Data Lake"};
            
            resourceTypes = new List<string>();

            foreach (var role in local)
            {
                resourceTypes.Add(role);
            }

            logger.LogInformation($"Azure resource Types created successfully");
        }

        private async Task LoadUsersAsync()
        {
            var usertypes = new[] { "AD user", "Guest" };

            var rand = new Bogus.Randomizer();

            var fakeUsers= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.User}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => f.Name.FirstName())
                .RuleFor(o => o.Type, f => $"{AzureItemType.User}")
                .RuleFor(o => o.Surname, f => f.Name.LastName())
                .RuleFor(o => o.UserType, f => f.PickRandom(usertypes))
                .RuleFor(o => o.Mail, f => f.Internet.Email())
                .RuleFor(o => o.Department, f=>f.Commerce.Department())
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath())
                .RuleFor(o=> o.ImgUrl, f=>f.Image.PicsumUrl());

               
            int max = QUANTITY_USERS;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeUsers.Generate();

                wiw = await base.InsertOrMergeEntityAsync(wiw);
                users.Add(wiw);

                logger.LogInformation($"User added successfully {wiw.ToString()}");
            }
        }

        private async Task LoadTags()
        {
            var rand = new Bogus.Randomizer();

            var fakeTags = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.Tag}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"CLID:{rand.Replace("##-####-####")}")
                .RuleFor(o => o.Type, f => $"{AzureItemType.Tag}");
            
            int max = QUANTITY;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeTags.Generate();

                wiw = await InsertOrMergeEntityAsync(wiw);
                tags.Add(wiw);
                
                logger.LogInformation($"Tag added successfully {wiw.ToString()}");
            }
        }

        private async Task LoadGroups()
        {
            var groupType = new[] { "Mail Enabled", "Security" };

            var rand = new Bogus.Randomizer();

            var fakeGroups= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.Group}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"Group {f.Commerce.ProductName()} {f.UniqueIndex}")
                .RuleFor(o => o.Type, f => $"{AzureItemType.Group}")
                .RuleFor(o => o.GroupType, f => f.PickRandom(groupType))
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath())
                .RuleFor(o=> o.ImgUrl, f=>f.Image.PicsumUrl());

            var fakeUserInGroup = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.UserInGroup}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.ChildPartitionKey, f => f.PickRandom(users).PartitionKey)
                .RuleFor(o => o.ChildRowKey, f => f.PickRandom(users).RowKey)
                .RuleFor(o => o.Type, f => $"{AzureItemType.UserInGroup}");


            int max = QUANTITY;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeGroups.Generate();
                wiw = await InsertOrMergeEntityAsync(wiw);
                groups.Add(wiw);

                logger.LogInformation($"Group added successfully {wiw.ToString()}");

                int usersInGroup = rand.Int(3,5);
                for (int j=0; j<usersInGroup; j++)
                {
                    var uig = fakeUserInGroup.Generate();
                    uig.ParentPartitionKey = wiw.PartitionKey;
                    uig.ParentRowKey = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync(uig);
                    logger.LogInformation($"User In Group {uig.ToString()}");
                }
            }
        }

        private async Task LoadSubscriptions()
        {
            var rand = new Bogus.Randomizer();

            var fakeSubscriptions= new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.Subscription}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.Name, f => $"subscription {f.Commerce.ProductMaterial()} {f.UniqueIndex}")
                .RuleFor(o => o.Type, f => $"{AzureItemType.Subscription}")
                .RuleFor(o=> o.DeepLink, f=>f.Internet.UrlWithPath());

            var fakeUserInSubscription = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.UserInSubscription}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.ChildPartitionKey, f => f.PickRandom(users).PartitionKey)
                .RuleFor(o => o.ChildRowKey, f => f.PickRandom(users).RowKey)
                .RuleFor(o => o.Type, f => $"{AzureItemType.UserInSubscription}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

            var fakeGroupInSubscription = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.GroupInSubscription}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.ChildPartitionKey, f => f.PickRandom(groups).PartitionKey)
                .RuleFor(o => o.ChildRowKey, f => f.PickRandom(groups).RowKey)
                .RuleFor(o => o.Type, f => $"{AzureItemType.GroupInSubscription}")
                .RuleFor(o => o.Name, f => f.PickRandom(roles));

             var fakeTagInSubscription = new Faker<WhoIsWhoEntity>()
                .StrictMode(false)
                .RuleFor(o => o.PartitionKey, f => $"{AzureItemType.TagInSubscription}{f.UniqueIndex % 10}")
                .RuleFor(o => o.RowKey, f => $"{f.UniqueIndex}")
                .RuleFor(o => o.ChildPartitionKey, f => f.PickRandom(tags).PartitionKey)
                .RuleFor(o => o.ChildRowKey, f => f.PickRandom(tags).RowKey)
                .RuleFor(o => o.Type, f => $"{AzureItemType.TagInSubscription}");

            int max = QUANTITY;
            for (int i=0; i<max; i++)
            {
                var wiw = fakeSubscriptions.Generate();
                wiw = await InsertOrMergeEntityAsync(wiw);
                subscriptions.Add(wiw);

                logger.LogInformation($"Subscription added successfully {wiw.ToString()}");

                int usersInSubscriptions = rand.Int(3,5);
                for (int j=0; j<usersInSubscriptions; j++)
                {
                    var uig = fakeUserInSubscription.Generate();
                    uig.ParentPartitionKey = wiw.PartitionKey;
                    uig.ParentRowKey = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync(uig);
                    logger.LogInformation($"User In Subscription {uig.ToString()}");
                }

                int groupsInSubscriptions = rand.Int(3,5);
                for (int j=0; j<groupsInSubscriptions; j++)
                {
                    var uig = fakeGroupInSubscription.Generate();
                    uig.ParentPartitionKey = wiw.PartitionKey;
                    uig.ParentRowKey = wiw.RowKey;

                    uig = await InsertOrMergeEntityAsync(uig);
                    logger.LogInformation($"Group In Subscription {uig.ToString()}");
                }

                 int tagsInSunscription = rand.Int(3,5);
                for (int j=0; j<tagsInSunscription; j++)
                {
                    var tig = fakeTagInSubscription.Generate();
                    tig.ParentPartitionKey = wiw.PartitionKey;
                    tig.ParentRowKey = wiw.RowKey;

                    tig = await InsertOrMergeEntityAsync(tig);
                    logger.LogInformation($"Tag In Subscription {tig.ToString()}");
                }
            }
        }
    }
}
