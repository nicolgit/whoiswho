using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;

namespace WhoIsWho.DataLoader.Models
{
    public class WhoIsWhoEntity : TableEntity
    {
        public WhoIsWhoEntity()
        {
        }
        public WhoIsWhoEntity(string partitionKey, string id)
        {
            RowKey = id;
            PartitionKey = partitionKey.ToString();
            Type = partitionKey;
        }

        public string DeepLink { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string GroupType { get; set; }
        public string ResourceType { get; set; }
        public string Department { get; set; }
        public string Mail { get; set; }
        public string Surname { get; set; }
        public string UserType { get; set; }
        public string ImgUrl { get; set; }

        //Relation fields
        public string ParentPartitionKey { get; set; }
        public string ParentRowKey { get; set; }
        public string ChildPartitionKey { get; set; }
        public string ChildRowKey { get; set; }

        public string ToString(bool verbose = false)
        {
            return verbose ?
             JsonConvert.SerializeObject(this, Formatting.None) :
             $"{PartitionKey}|{RowKey} - {Name} - {ParentPartitionKey}{ParentRowKey}|{ChildPartitionKey}{ChildRowKey}";
        }
    }
}
