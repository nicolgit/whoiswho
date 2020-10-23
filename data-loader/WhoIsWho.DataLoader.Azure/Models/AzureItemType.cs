using System;
using System.Collections.Generic;
using System.Text;

namespace WhoIsWho.DataLoader.Azure.Models
{
    public enum AzureItemType
    {
        Tag,
        User,
        Group,
        UserInGroup,
        Subscription,
        UserInSubscription,
        GroupInSubscription,
        ResourceGroup,
        UserInResourceGroup,
        GroupInResourceGroup,
        Resource,
        UserInResource,
        GroupInResource,
        Application,
        UserInApplication,
        GroupInApplication,
        ResourceGroupInSubscription, // to be implemented
        ResourceInResourceGroup, // to be implemented
        TagInResource,
        TagInResourceGroup,
        TagInSubscription,
        TagInApplication
    };
}
