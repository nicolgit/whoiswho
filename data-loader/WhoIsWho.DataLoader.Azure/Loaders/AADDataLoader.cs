using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Azure.Models;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Azure.Loaders
{
    public class AADDataLoader : BaseDataLoader
    {
        private readonly ILogger logger;
        private readonly IConfiguration config;
        private readonly IGraphServiceClient graphClient;
        public AADDataLoader(IConfiguration configuration,
            ILogger<AADDataLoader> logger,
            IGraphServiceClient graphclient) : base(configuration, logger, nameof(AADDataLoader))
        {
            this.logger = logger;
            this.config = configuration;
            this.graphClient = graphclient;
        }


        public override async Task LoadDataAsync()
        {
            var users = await GetUsers().ConfigureAwait(false);
            var groups = await GetGroups().ConfigureAwait(false);

            IEnumerable<WhoIsWhoEntity> usersWho = users.Select(u => new WhoIsWhoEntity(AzureItemType.User.ToString(), u.Id)
            {
                GivenName = u.GivenName,
                Surname = u.Surname,
                Name = u.DisplayName,
                Mail = u.Mail,
                Department = u.Department,
                UserType = u.UserType
            });

            IEnumerable<WhoIsWhoEntity> groupsWho = groups.Select(g => new WhoIsWhoEntity(AzureItemType.Group.ToString(), g.Id)
            {
                Name = g.DisplayName,
                Description = g.Description,
                Mail = g.Mail,
                GroupType = GetGroupType(g.GroupTypes),

            });

            await Task.WhenAll(InsertOrMergeEntitiesBatchAsync(usersWho), InsertOrMergeEntitiesBatchAsync(groupsWho));
        }

        private static string GetGroupType(IEnumerable<string> groupTypes)
        {
            return "TODO";
        }

        private async Task<IEnumerable<User>> GetUsers()
        {
            var users = new List<User>();
            try
            {
                var usersCollection = await graphClient.Users.Request()
                    .Select("id,givenName,surname,displayName,mail,department,userType")
                    .GetAsync()
                    .ConfigureAwait(false);
                if (usersCollection != null && usersCollection.Any())
                {
                    users.AddRange(usersCollection);
                    while (usersCollection.NextPageRequest != null)
                    {
                        usersCollection = await usersCollection.NextPageRequest.GetAsync().ConfigureAwait(false);
                        users.AddRange(usersCollection);
                    }
                }
            }
            catch (System.Exception e)
            {
                logger.LogError(e, "Unable to retrieve all users from AAD. Result may be partial.");
            }

            return users;
        }

        private async Task<IEnumerable<Group>> GetGroups()
        {
            var groups = new List<Group>();
            try
            {
                var groupsCollection = await graphClient.Groups.Request()
                    .Select("id,displayName,description,groupTypes,mailEnabled,securityEnabled,mail")
                    .GetAsync().ConfigureAwait(false);
                if (groupsCollection != null && groupsCollection.Any())
                {
                    groups.AddRange(groupsCollection);
                    while (groupsCollection.NextPageRequest != null)
                    {
                        groupsCollection = await groupsCollection.NextPageRequest.GetAsync().ConfigureAwait(false);
                        groups.AddRange(groupsCollection);
                    }
                }
            }
            catch (System.Exception e)
            {
                logger.LogError(e, "Unable to retrieve all groups from AAD. Result may be partial.");
            }

            return groups;
        }
    }
}
