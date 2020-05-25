namespace fake_data_loader.Model
{
    using System;
    using Microsoft.Azure.Cosmos.Table;
    using Newtonsoft.Json;

    public enum ItemType
    {
        Tag = 0,
        User = 1,
        Group = 2,
        UserInGroup = 3,
        Subscription = 4,
        UserGroupInSubscription=5,
        ResourceGroup = 6,
        UserGroupInResourceGroup = 7,
        Resource = 8,
        UserGroupInResource = 9,
        Application = 10,
        UserGroupInApplication = 11
    };

    public class WhoIsWhoEntity : TableEntity
    {
        public WhoIsWhoEntity()
        {
        }

        public WhoIsWhoEntity(ItemType type, string id)
        {
            RowKey = id;
            PartitionKey = type.ToString() + "-" +RowKey.Substring(1,2);

            Type = type.ToString();
        }

        public string DeepLink { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mail { get; set; }
        public string Department { get; set; }
        public string UserType { get; set; }
        public string GroupType { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroupId { get; set; }
        public string ResourceId { get; set; }
        public string UserId { get; set; }
        public string GroupId { get; set; }
        public string ApplicationId { get; set; }
        public string ImgUrl { get; set; }
        public string ResourceType { get; set; }
        

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}