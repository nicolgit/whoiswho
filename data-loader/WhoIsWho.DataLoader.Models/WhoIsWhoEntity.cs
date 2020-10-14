using Microsoft.Azure.Cosmos.Table;
using System;

namespace WhoIsWho.DataLoader.Models
{
    public class WhoIsWhoEntity : TableEntity
    {
        public WhoIsWhoEntity(string type, string id)
        {
            RowKey = id;
            PartitionKey = type.ToString();
            Type = type;
            TimeStamp = DateTime.Now;
        }

        public DateTime TimeStamp { get; set; }
        public string DeepLink { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string GroupType { get; set; }
        public string ResourceType { get; set; }
        public string Department { get; set; }
        public string Mail { get; set; }
        public string Surname { get; set; }
        public string UserType { get; set; }


        //Relation fields
        public string ParentPartitionKey { get; set; }
        public string ParentRowKey { get; set; }
        public string ChildPartitionKey { get; set; }
        public string ChildRowKey { get; set; }
    }
}
