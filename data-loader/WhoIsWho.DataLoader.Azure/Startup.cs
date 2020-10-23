using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
            }
        }
    }
}