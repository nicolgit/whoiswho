using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using WhoIsWho.DataLoader.Azure.Loaders;

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

				var servProv = builder.Services.BuildServiceProvider();
				IConfiguration config = servProv.GetRequiredService<IConfiguration>();

				builder.Services.AddSingleton<IGraphServiceClient>(sp =>
				{
					var confClient = ConfidentialClientApplicationBuilder.Create(config["GraphClient:client_id"])
					.WithTenantId(config["GraphClient:tenant_id"])
					.WithClientSecret(config["GraphClient:client_secret"])
					.Build();
					ClientCredentialProvider clientCredentialProvider = new ClientCredentialProvider(confClient);
					return new GraphServiceClient(clientCredentialProvider);
				});
			}
		}
	}
}