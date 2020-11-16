using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using WhoIsWho.DataLoader.Sync.Services;
using WhoIsWho.DataLoader.Sync.Services.Abstract;

[assembly: FunctionsStartup(typeof(WhoIsWho.DataLoader.Sync.Startup))]
namespace WhoIsWho.DataLoader.Sync
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder != null)
            {
                builder.Services.AddSingleton<IWhoIsWhoDataSyncronizer, WhoIsWhoDataSyncronizer>();
            }
        }
    }
}