using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Azure.Loaders
{
    public class AADDataLoader : BaseDataLoader
    {
        private readonly ILogger logger;

        public AADDataLoader(IConfiguration configuration, ILogger<AADDataLoader> logger) : base(configuration, logger, nameof(AADDataLoader))
        {
            this.logger = logger;
        }


        public override async Task LoadData()
        {

            //To do
        }

    }
}
