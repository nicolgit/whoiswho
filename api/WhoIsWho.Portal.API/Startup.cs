using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using WhoIsWho.Portal.Api.Services;

[assembly: FunctionsStartup(typeof(WhoIsWho.Portal.API.Startup))]
namespace WhoIsWho.Portal.API
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
                builder.Services.AddHttpClient();
                builder.Services.AddSingleton<CognitiveSearchService>();
            }
        }

    }
}
