using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;

namespace WhoIsWho.DataLoader.Models
{
    public class WhoIsWhoSyncedEntity : WhoIsWhoEntity
    {
        public bool IsDeleted{ get; set; }
    }
}
