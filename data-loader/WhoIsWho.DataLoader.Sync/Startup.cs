using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Models.AutoMapperProfiles;
using WhoIsWho.DataLoader.Sync.Services;

[assembly: FunctionsStartup(typeof(WhoIsWho.DataLoader.Sync.Startup))]
namespace WhoIsWho.DataLoader.Sync
{
    class Startup : FunctionsStartup
    {
        public Startup()
        {
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder != null)
            {
                builder.Services.AddAutoMapper(typeof(WhoIsWhoEntityProfile));
                builder.Services.AddTransient<IWhoIsWhoDataSyncronizer, WhoIsWhoDataSyncronizer>();
                builder.Services.AddScoped<IWhoIsWhoDataReader, WhoIsWhoDataReader>();
                builder.Services.AddApplicationInsightsTelemetry();
            }
        }
    }
}