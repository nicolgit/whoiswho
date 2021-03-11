using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using WhoIsWho.DataLoader.Azure.Loaders;
using WhoIsWho.DataLoader.Azure.Utility;

[assembly: FunctionsStartup(typeof(WhoIsWho.DataLoader.Azure.Startup))]
namespace WhoIsWho.DataLoader.Azure
{
    class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			if (builder != null)
			{
				builder.Services.AddSingleton<AADDataLoader>();
				builder.Services.AddSingleton<ARMDataLoader>();
				builder.Services.AddSingleton<FakeDataLoader>();
				
				var servProv = builder.Services.BuildServiceProvider();
				IConfiguration config = servProv.GetRequiredService<IConfiguration>();

				var confClient = ConfidentialClientApplicationBuilder.Create(config["ServicePrincipalClientID"])
					.WithTenantId(config["ServicePrincipalTenantID"])
					.WithClientSecret(config["ServicePrincipalClientSecret"])
					.Build();

				builder.Services.AddSingleton<IGraphServiceClient>(sp =>
				{
					ClientCredentialProvider clientCredentialProvider = new ClientCredentialProvider(confClient);
					return new GraphServiceClient(clientCredentialProvider);
				});

				var customTokenProvider = new CustomTokenProvider(confClient, "https://management.core.windows.net/.default");

				builder.Services.AddSingleton<Microsoft.Azure.Management.Subscription.ISubscriptionClient>(sp =>
				{
					return new Microsoft.Azure.Management.Subscription.SubscriptionClient(new TokenCredentials(customTokenProvider));
				});

				builder.Services.AddSingleton<IAuthorizationManagementClient>(sp =>
				{
					return new AuthorizationManagementClient(new TokenCredentials(customTokenProvider));
				});

				builder.Services.AddSingleton<IResourceManagementClient>(sp =>
				{
					return new ResourceManagementClient(new TokenCredentials(customTokenProvider));
				});


			}
		}
	}
}