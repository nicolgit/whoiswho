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

        public AADDataLoader(IConfiguration configuration, ILogger<AADDataLoader> logger) : base(configuration, logger)
        {
            this.logger = logger;
        }

        public override string LoaderIdentifier => nameof(AADDataLoader);

        public override async Task LoadData()
        {

            //To do
        }

    }
}
