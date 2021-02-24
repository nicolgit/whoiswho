# Who Is Who
![Build WhoIsWho](https://github.com/nicolgit/whoiswho/workflows/Build%20WhoIsWho/badge.svg)

Who Is Who allows you to index all your IT assets: Azure Resources, Azure Active Directory and elements of other systems.
You can leverage the Who Is Who full-text search to find all you need and retrieve the relationships betweeb the different elements.

## Prerequisites
You need to create an Azure AD Service Principal that will represent the application identity during the execution. This principal can be assigned to the Azure Resources (ex.Subscription,Resource Group, AppService, etc.) that you want the solution will index. The same principal should be set with the right permission on Azure Ad.
``` bash
az ad sp create-for-rbac --name {appRegistrationName} --years {numberOfTheYearOfExpirationForGeneratedPassword} --skip-assignment
```
and take note of the output JSON that should look like this:
``` javascript
{
"appId": "xxxxxxx-d574-47c3-a84b-yyyyyyyyy",
"password": "yourSecret",
"displayName": "yourSubscriptionId",
"tenant": "yourAADTenantId",
}
```
From now you can assign the Azure AD Service Principal identified by the displayName to every Azure Resource via RBAC with the "Reader" role assignment. 

## Deployment
### Create Resource Group and a Service Principal for deploy the resources
You can deploy the solution via the ARM Template provided with this repo by executing the GitHub Action named "Deploy WhoIsWho". You will need to create an Azure Resource Group that will contain the resources and a Service Principal that will have the Contribution permission to it used by the GitHub Action for the deploy the resources and the code. You can use the following instructions: 
1. Create a Resource Group on Azure
2. Create an Azure AD App Registration/Service Principal. You can use the Azure AD functionalities from the portal or launch the followind "az cli" command:

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

### Deploy Infrastracture and Apps

1. Create the following GitHub secrets:

| SecretName| Content |
| --- | --- |
| yourname_appregistration | Store inside the App Registration output JSON |
| yourname_subscription | Store inside the subscription id |

2. Launch manually the GitHub action named 'Deploy WhoIsWho' with the following parameters:

| Parameter | Value |
| --- | --- |
| Azure Secret Name - Service Principal | The name of the GitHub secret that stores the App Registration output JSON |
| Azure Secret Name - Subscription | The name of the GitHub secret that stores the the subscription id |
| Resource Group Name | The name of resource group |
| Resource Location | The resources location |
| Resources Name Main Identifier | The string that will identify uniquely all the Azure Resources that will be created, ex. if set to the value **'mywhoiswho'** deploy, the following resources will be created: app-**mywhoiswho**, appi-**mywhoiswho**, func-**mywhoiswho**-azureloader, func-**mywhoiswho**-datasync, plan-**mywhoiswho**, srch-**mywhoiswho**|
| Storage Account Name | The name of the storage account that will be created and support the previous resources |

3. Wait that the deploument will be completed
   
