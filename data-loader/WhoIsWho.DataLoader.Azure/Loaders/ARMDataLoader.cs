using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.Subscription;
using Microsoft.Azure.Management.Subscription.Models;
using Microsoft.Azure.Services.AppAuthentication;
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
using SubscriptionClient = Microsoft.Azure.Management.Subscription.SubscriptionClient;

namespace WhoIsWho.DataLoader.Azure.Loaders
{
    public class ARMDataLoader : BaseDataLoader
    {
        private readonly ILogger logger;

        SubscriptionClient subscriptionClient;
        AuthorizationManagementClient authorizationManagementClient;
        ResourceManagementClient resourceManagementClient;

        List<SubscriptionModel> currentSubscriptions = new List<SubscriptionModel>();

        public ARMDataLoader(IConfiguration configuration, ILogger<ARMDataLoader> logger) : base(configuration, logger, nameof(ARMDataLoader))
        {
            this.logger = logger;
        }

        async Task InitializeClientsWithCredentialsAsync()
        {
            var aadToken = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://management.core.windows.net/");
            ServiceClientCredentials serviceClientCreds = new TokenCredentials(aadToken);
            subscriptionClient = new SubscriptionClient(serviceClientCreds);
            authorizationManagementClient = new AuthorizationManagementClient(serviceClientCreds);
            resourceManagementClient = new ResourceManagementClient(serviceClientCreds);
        }

        public override async Task LoadData()
        {
            await InitializeClientsWithCredentialsAsync();
            await LoadSubscriptionsAsync();
            await LoadRoleAssignmentsAsync();
            await LoadResourceGroups();
            await LoadResourcesAsync();
        }

        private async Task LoadSubscriptionsAsync()
        {
            var subscriptions = GetIterator(await subscriptionClient.Subscriptions.ListAsync(), async x => await subscriptionClient.Subscriptions.ListNextAsync(x));

            await foreach (var currentSubscription in subscriptions)
            {
                currentSubscriptions.Add(currentSubscription);

                var subscription = new WhoIsWhoEntity(AzureItemType.Subscription.ToString(), currentSubscription.SubscriptionId)
                {
                    Name = currentSubscription.DisplayName,
                };
                await base.InsertOrMergeEntityAsync(subscription);
            }

        }


        private async Task LoadRoleAssignmentsAsync()
        {
            var iter = GetIterator(await authorizationManagementClient.RoleDefinitions.ListAsync(string.Empty), async x => await authorizationManagementClient.RoleDefinitions.ListNextAsync(x));
            var roles = await iter.ToListAsync();

            foreach (var currentSubscription in currentSubscriptions)
            {
                authorizationManagementClient.SubscriptionId = currentSubscription.SubscriptionId;
                var assignments = GetIterator(await authorizationManagementClient.RoleAssignments.ListAsync(), async x => await authorizationManagementClient.RoleAssignments.ListNextAsync(x));
                await foreach (var currentAssignment in assignments)
                {
                    var currentRoleDescription = roles.Find(x => x.Id == currentAssignment.RoleDefinitionId.Substring(currentAssignment.RoleDefinitionId.IndexOf("/providers/Microsoft.Authorization/roleDefinitions"))).RoleName;
                    string assignmentType = string.Empty;
                    string childPartitionKey = string.Empty;
                    string childRowKey = string.Empty;
                    var resourceMatch = Regex.Match(currentAssignment.Scope.ToLower(), @"\/subscriptions\/(?s)(.*)\/resourcegroups\/(?<ResourceGroup>(?s)(.*))\/providers\/(?s)(.*)");
                    if (resourceMatch.Success)
                    {
                        assignmentType = AzureItemType.UserInResource.ToString();
                        childPartitionKey = AzureItemType.Resource.ToString();
                        childRowKey = FormatResourceKey(currentSubscription.SubscriptionId, resourceMatch.Groups["ResourceGroup"].Value, currentAssignment.Scope.Substring(currentAssignment.Scope.LastIndexOf("/") + 1));
                    }
                    else
                    {
                        var resourceGroupMatch = Regex.Match(currentAssignment.Scope.ToLower(), @"\/subscriptions\/(?s)(.*)\/resourcegroups\/(?<ResourceGroup>(?s)(.*))");
                        if (resourceGroupMatch.Success)
                        {
                            assignmentType = AzureItemType.UserInResourceGroup.ToString();
                            childPartitionKey = AzureItemType.ResourceGroup.ToString();
                            childRowKey = FormatResourceGroupKey(currentSubscription.SubscriptionId, resourceGroupMatch.Groups["ResourceGroup"].Value);
                        }
                        else if (Regex.IsMatch(currentAssignment.Scope.ToLower(), @"\/subscriptions\/(?s)(.*)"))
                        {
                            assignmentType = AzureItemType.UserInSubscription.ToString();
                            childPartitionKey = AzureItemType.Subscription.ToString();
                            childRowKey = currentSubscription.SubscriptionId;
                        }
                        else
                        {
                            //TO DO: Evaluate other scopes (example user in management groups or gloabal)
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(assignmentType))
                    {
                        var userInSubscription = new WhoIsWhoEntity(assignmentType, currentAssignment.Id.Substring(currentAssignment.Id.LastIndexOf("/") + 1))
                        {
                            Name = currentRoleDescription,
                            ParentPartitionKey = AzureItemType.User.ToString(),
                            ParentRowKey = currentAssignment.PrincipalId,
                            ChildPartitionKey = childPartitionKey,
                            ChildRowKey = childRowKey
                        };
                        await base.InsertOrMergeEntityAsync(userInSubscription);
                    }
                }
            }
        }

        private async Task LoadResourceGroups()
        {
            foreach (var currentSubscription in currentSubscriptions)
            {
                resourceManagementClient.SubscriptionId = currentSubscription.SubscriptionId;
                var resourceGroups = GetIterator(await resourceManagementClient.ResourceGroups.ListAsync(), async x => await resourceManagementClient.ResourceGroups.ListNextAsync(x));
                await foreach (var currentResourceGroup in resourceGroups)
                {
                    var resourceGroup = new WhoIsWhoEntity(AzureItemType.ResourceGroup.ToString(), FormatResourceGroupKey(currentSubscription.SubscriptionId, currentResourceGroup.Name))
                    {
                        Name = currentResourceGroup.Name,
                    };
                    await base.InsertOrMergeEntityAsync(resourceGroup);

                    var resourceGroupinSubscription = new WhoIsWhoEntity(AzureItemType.ResourceGroupInSubscription.ToString(), FormatResourceGroupKey(currentSubscription.SubscriptionId, currentResourceGroup.Name))
                    {
                        ParentPartitionKey = AzureItemType.ResourceGroup.ToString(),
                        ParentRowKey = FormatResourceGroupKey(currentSubscription.SubscriptionId, currentResourceGroup.Name),
                        ChildPartitionKey = AzureItemType.Subscription.ToString(),
                        ChildRowKey = currentSubscription.SubscriptionId
                    };
                    await base.InsertOrMergeEntityAsync(resourceGroupinSubscription);
                }
            }
        }

        private async Task LoadResourcesAsync()
        {
            foreach (var currentSubscription in currentSubscriptions)
            {
                resourceManagementClient.SubscriptionId = currentSubscription.SubscriptionId;
                var resources = GetIterator(await resourceManagementClient.Resources.ListAsync(), async x => await resourceManagementClient.Resources.ListNextAsync(x));
                await foreach (var currentResource in resources)
                {
                    var resourceIdComponents = Regex.Match(currentResource.Id.ToLower(), @"\/subscriptions\/(?s)(.*)\/resourcegroups\/(?<ResourceGroup>(?s)(.*))\/providers\/(?s)(.*)");
                    var resourceGroupName = resourceIdComponents.Groups["ResourceGroup"].Value;

                    var resourceId = currentResource.Id.Substring(currentResource.Id.LastIndexOf("/") + 1);
                    var resourceKey = FormatResourceKey(currentSubscription.SubscriptionId, resourceGroupName, resourceId);
                    var resource = new WhoIsWhoEntity(AzureItemType.Resource.ToString(), resourceKey)
                    {
                        Name = currentResource.Name,
                        ResourceType = currentResource.Type
                    };
                    await base.InsertOrMergeEntityAsync(resource);


                    var resourceInRG = new WhoIsWhoEntity(AzureItemType.ResourceInResourceGroup.ToString(), resourceKey)
                    {
                        ParentPartitionKey = AzureItemType.Resource.ToString(),
                        ParentRowKey = resourceKey,
                        ChildPartitionKey = AzureItemType.ResourceGroup.ToString(),
                        ChildRowKey = FormatResourceGroupKey(currentSubscription.SubscriptionId, resourceGroupName)
                    };
                    await base.InsertOrMergeEntityAsync(resourceInRG);
                }
            }
        }

        async IAsyncEnumerable<T> GetIterator<T>(IPage<T> firstPage, Func<string, Task<IPage<T>>> getNextPage)
        {
            var currentPage = firstPage;
            do
            {
                foreach (var currentElement in currentPage)
                {
                    yield return currentElement;
                }
                if (!string.IsNullOrWhiteSpace(currentPage.NextPageLink))
                    currentPage = await getNextPage(currentPage.NextPageLink);
                else
                    currentPage = null;
            } while (currentPage != null);
        }

        string FormatResourceGroupKey(string subscription, string resourceGroup)
        {
            return $"{subscription}_{resourceGroup}";
        }

        string FormatResourceKey(string subscription, string resourceGroup, string resourceId)
        {
            return $"{subscription}_{resourceGroup}_{resourceId}";
        }

    }
}
