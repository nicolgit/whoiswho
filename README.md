# Who Is Who
![Build WhoIsWho](https://github.com/nicolgit/whoiswho/workflows/Build%20WhoIsWho/badge.svg)

Who Is Who allows you to index all your IT assets: Azure Resources, Azure Active Directory and elements of other systems.
You can leverage the Who Is Who full-text search to find all you need and retrieve the relationships betweeb the different elements.

## Deployment
### Setup Azure Prerequisites
1. Create a Resource Group on Azure
2. Create an App Registration/Service Principal. You can use the Azure AD functionalities from the portal or launch the followind "az cli" command:

``` bash
az ad sp create-for-rbac --name {appRegistrationName} --role contributor --scopes /subscriptions/{subscriptionID}/resourceGroups/{resourceGroupName} --sdk-auth

```
 and take note of the output JSON that should look like this:

``` javascript
{
"clientId": "xxxxxxx-d574-47c3-a84b-yyyyyyyyy",
"clientSecret": "yourSecret",
"subscriptionId": "yourSubscriptionId",
"tenantId": "yourAADTenantId",
...
"managementEndpointUrl": "[https://management.core.windows.net/](https://management.core.windows.net/)"
}
```
3. Add the previous created App Registration with the Contributor role on the created Resource Group.

### Deploy Infrastracture and Apps

1. Create the following GitHub secrets:

| SecretName| Content |
| --- | --- |
| yourname_appregistration | Store inside the App Registration output JSON |
| yourname_subscription | Store inside the subscription id |

2. Launch manually the GitHub action named 'Deploy WhoIsWho' with the following parameters:

| Parameter &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; | Value |
| --- | --- |
| Azure Secret Name - Service Principal | The name of the GitHub secret that stores the App Registration output JSON |
| Azure Secret Name - Subscription | The name of the GitHub secret that stores the the subscription id |
| Resource Group Name | The name of resource group |
| Resource Location | The resources location |
| Resources Name Main Identifier | The string that will identify uniquely all the Azure Resources that will be created, ex. if set to the value **'mywhoiswho'** deploy, the following resources will be created: app-**mywhoiswho**, appi-**mywhoiswho**, func-**mywhoiswho**-azureloader, func-**mywhoiswho**-datasync, plan-**mywhoiswho**, srch-**mywhoiswho**|
| Storage Account Name | The name of the storage account that will be created and support the previous resources |

3. Wait that the deploument will be completed
   
## Configure the application
Coming soon....
