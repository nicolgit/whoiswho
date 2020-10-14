using Microsoft.Azure.Management.Authorization;
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
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Azure.Loaders
{
    public class ARMDataLoader : BaseDataLoader
    {
        private readonly ILogger logger;

        SubscriptionClient subscriptionClient;
        AuthorizationManagementClient authorizationManagementClient;

        List<SubscriptionModel> currentSubscriptions = new List<SubscriptionModel>();

        public ARMDataLoader(IConfiguration configuration, ILogger<ARMDataLoader> logger) : base(configuration, logger)
        {
            this.logger = logger;
        }

        async Task InitializeClientsWithCredentialsAsync()
        {
            var aadToken = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://management.core.windows.net/");
            ServiceClientCredentials serviceClientCreds = new TokenCredentials(aadToken);
            subscriptionClient = new SubscriptionClient(serviceClientCreds);
            authorizationManagementClient = new AuthorizationManagementClient(serviceClientCreds);
        }

        public override string LoaderIdentifier => nameof(ARMDataLoader);

        public override async Task LoadData()
        {
            await InitializeClientsWithCredentialsAsync();
            await LoadSubscriptionsAsync();
            await LoadRoleAssignmentsAsync();

        }

        private async Task LoadSubscriptionsAsync()
        {
            var subscriptions = GetIterator(await subscriptionClient.Subscriptions.ListAsync(), async x => await subscriptionClient.Subscriptions.ListNextAsync(x));
            await foreach (var currentSubscription in subscriptions)
            {
                currentSubscriptions.Add(currentSubscription);
            }
        }


        private async Task LoadRoleAssignmentsAsync()
        {
            foreach (var currentSubscription in currentSubscriptions)
            {
                var assignments = GetIterator(await authorizationManagementClient.RoleAssignments.ListAsync(), async x => await authorizationManagementClient.RoleAssignments.ListNextAsync(x));
                await foreach (var currentAssignment in assignments)
                {
                    currentSubscriptions.Add(currentSubscription);
                }

            }

            var d = new WhoIsWhoEntity("Test", "1");
            await base.InsertOrMergeEntityAsync(d);
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


    }
}
