using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.Subscription;
using Microsoft.Azure.Management.Subscription.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Azure.Models;
using WhoIsWho.DataLoader.Core;
using WhoIsWho.DataLoader.Models;
using SubscriptionClient = Microsoft.Azure.Management.Subscription.SubscriptionClient;

namespace WhoIsWho.DataLoader.Azure.Loaders
{
    public class FakeDataLoader : BaseDataLoader
    {
        private readonly ILogger logger;

        
        public FakeDataLoader(IConfiguration configuration, ILogger<ARMDataLoader> logger) : base(configuration, logger)
        {
            this.logger = logger;
        }

        public int Number { get; set; }
        public override string LoaderIdentifier => nameof(ARMDataLoader);

        
        public override async Task LoadData()
        {
            
        }
    }
}
