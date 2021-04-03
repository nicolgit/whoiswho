using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using WhoIsWho.Api.services;


namespace WhoIsWho.Api
{
    public class Program
    {
        public static void Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(s => { 
                    s.AddSingleton<CognitiveSearchService>();
                    s.AddHttpClient();  
                })
                .Build();

            host.Run();
        }
    }
}