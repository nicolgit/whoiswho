using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.OData;
using System;
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

			var usersInGroup = await GetUsersInGroup(users).ConfigureAwait(false);
			var groupsInGroup = await GetGroupsInGroup(groups).ConfigureAwait(false);
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
				GroupType = GetGroupType(g.GroupTypes, g.MailEnabled, g.SecurityEnabled)
			});

			Task[] tasks = new Task[] {
				InsertOrMergeEntitiesBatchAsync(usersWho),
				InsertOrMergeEntitiesBatchAsync(groupsWho),
				InsertOrMergeEntitiesBatchAsync(usersInGroup),
				InsertOrMergeEntitiesBatchAsync(groupsInGroup)
			};

			await Task.WhenAll(tasks);
		}

		private async Task<IEnumerable<WhoIsWhoEntity>> GetUsersInGroup(IEnumerable<User> users)
		{
			if (users == null)
				throw new ArgumentNullException(nameof(users));

			List<WhoIsWhoEntity> usersInGroups = new List<WhoIsWhoEntity>();
			try
			{
				foreach (var user in users)
				{
					List<DirectoryObject> groupsMemberOf = new List<DirectoryObject>();

					var memberOf = await graphClient.Users[user.Id].MemberOf.Request().GetAsync().ConfigureAwait(false);
					if (memberOf != null && memberOf.Any())
					{
						groupsMemberOf.AddRange(memberOf.Where(m => m.ODataType == "#microsoft.graph.group"));
						while (memberOf.NextPageRequest != null)
						{
							memberOf = await memberOf.NextPageRequest.GetAsync().ConfigureAwait(false);
							groupsMemberOf.AddRange(memberOf.Where(m => m.ODataType == "#microsoft.graph.group"));
						}
					}

					usersInGroups.AddRange(
							groupsMemberOf
							.Select(group => new WhoIsWhoEntity(AzureItemType.UserInGroup.ToString(), $"{user.Id}_{group.Id}")
							{
								ChildPartitionKey = AzureItemType.Group.ToString(),
								ChildRowKey = group.Id,
								ParentPartitionKey = AzureItemType.User.ToString(),
								ParentRowKey = user.Id
							})
							);
				}
			}
			catch (System.Exception e)
			{
				logger.LogError(e, "Unable to retrieve all groups memberships from AAD. Result may be partial.");
			}

			return usersInGroups;
		}
		private async Task<IEnumerable<WhoIsWhoEntity>> GetGroupsInGroup(IEnumerable<DirectoryObject> groups)
		{
			if (groups == null)
				throw new ArgumentNullException(nameof(groups));

			List<WhoIsWhoEntity> groupsInGroups = new List<WhoIsWhoEntity>();
			try
			{
				foreach (var group in groups)
				{
					List<DirectoryObject> groupsMemberOf = new List<DirectoryObject>();

					var memberOf = await graphClient.Groups[group.Id].MemberOf.Request().GetAsync().ConfigureAwait(false);
					if (memberOf != null && memberOf.Any())
					{
						groupsMemberOf.AddRange(memberOf.Where(m => m.ODataType == "#microsoft.graph.group"));
						while (memberOf.NextPageRequest != null)
						{
							memberOf = await memberOf.NextPageRequest.GetAsync().ConfigureAwait(false);
							groupsMemberOf.AddRange(memberOf.Where(m => m.ODataType == "#microsoft.graph.group"));
						}
					}

					groupsInGroups.AddRange(
							groupsMemberOf
							.Select(g => new WhoIsWhoEntity(AzureItemType.GroupInGroup.ToString(), $"{group.Id}_{g.Id}")
							{
								ChildPartitionKey = AzureItemType.Group.ToString(),
								ChildRowKey = g.Id,
								ParentPartitionKey = AzureItemType.Group.ToString(),
								ParentRowKey = group.Id
							})
							);
				}
			}
			catch (System.Exception e)
			{
				logger.LogError(e, "Unable to retrieve all groups memberships from AAD. Result may be partial.");
			}

			return groupsInGroups;
		}

		private static string GetGroupType(IEnumerable<string> groupTypes, bool? mailEnabled, bool? securityEnabled)
		{
			string groupType = "Unknown";

			//https://docs.microsoft.com/en-us/graph/api/resources/groups-overview?view=graph-rest-1.0
			if (groupTypes.Contains("Unified"))
			{
				if (mailEnabled.GetValueOrDefault())
					groupType = "Microsoft 365 Group";
			}
			else
			{
				if (securityEnabled.GetValueOrDefault())
				{
					if (!mailEnabled.GetValueOrDefault())
						groupType = "Security Group";
					else
						groupType = "Mail-enabled Security Group";
				}
				else if (mailEnabled.GetValueOrDefault())
					groupType = "Distribution Group";
			}

			return groupType;
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
