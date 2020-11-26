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
                var sourceTables = builder.GetContext().Configuration["SourceTableToSync"].Split(',');
                foreach (var table in sourceTables)
                {
                    builder.Services.AddScoped(x => new WhoIsWhoDataSyncronizer(
                    x.GetRequiredService<IConfiguration>(),
                    x.GetRequiredService<ILogger<WhoIsWhoDataSyncronizer>>(),
                    x.GetRequiredService<IWhoIsWhoDataReader>(),
                    x.GetRequiredService<IMapper>(),
                    table));
                }
                builder.Services.AddScoped<IWhoIsWhoDataReader, WhoIsWhoDataReader>();
            }
        }
    }
}