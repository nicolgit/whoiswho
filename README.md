# Who Is Who
![Build WhoIsWho](https://github.com/nicolgit/whoiswho/workflows/Build%20WhoIsWho/badge.svg)

Who Is Who allows you to index all your IT assets: Azure Resources, Azure Active Directory and elements of other systems.
You can leverage the Who Is Who full-text search to find all you need and retrieve the relationships betweeb the different elements.

# Deployment

 1. Create in your Azure Active Directory tenant the following 3 required service principals
	 1. **WhoIsWho Identity Deployment**, used by the GitHub action for the deployment:	 
		 1. Create a Resource Group on Azure
	    2. Create an Azure AD App Registration/Service Principal. You can use the Azure AD functionalities from the portal or launch the followind "az cli" command:
			``` bash
			az ad sp create-for-rbac --name "WhoIsWhoIdentityDeployment" --role contributor --scopes /subscriptions/{subscriptionID}/resourceGroups/{resourceGroupName} --sdk-auth
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
	 
	 2. **WhoIsWho Identity Backend**, exposes API to the frontend. This principal can be assigned to the Azure Resources (ex.Subscription,Resource Group, AppService, etc.) that you want the solution will index:
		``` bash
		az ad sp create-for-rbac --name "WhoIsWhoIdentityBackend --years {numberOfTheYearOfExpirationForGeneratedPassword} --skip-assignment
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
	 3. **WhoIsWho Identity Frontend**, represents the front-end and allows the user to authenticate:
		``` bash
		az ad sp create-for-rbac --name "WhoIsWhoIdentityFrontend" --years {numberOfTheYearOfExpirationForGeneratedPassword} --skip-assignment
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
2. Add and Grant the WhoIsWhoIdentityBackend "user_impersonation" permission to the WhoIsWhoIdentityFrontend service principal
``` bash
$appIdFrontend=az ad app list --display-name "WhoIsWhoIdentityFrontend" --query "[0].appId"
$appBackend= az ad app list --display-name "WhoIsWhoIdentityBackend" --query "{appId:[0].appId,permissionId:[0].oauth2Permissions[?value=='user_impersonation'] | [0].id}" | ConvertFrom-Json
az ad app permission add --id $appIdFrontend --api $appBackend.appId --api-permissions "$(${appBackend}.permissionId)=Scope"
az ad app permission grant --id $appIdFrontend --api $appBackend.appId
``` 

3. Create the following GitHub secrets:

| SecretName| Content |
| --- | --- |
| DEPLOYMENT_IDENTITY_APPREGISTRATION | Store inside the App Registration output JSON for the Deployment Identity |
| DEPLOYMENT_IDENTITY_SUBSCRIPTION | Store inside the subscription id for the Deployment Identity |
| WHOISWHO_IDENTITY_APPID | Store inside the appId of the Service Principal create for the WhoIsWho Identity Backend |
| WHOISWHO_IDENTITY_PASSWORD | Store inside the password of the Service Principal create for the WhoIsWho Identity Backend |
| WHOISWHO_IDENTITY_TENANT |  Store inside the tenant of the Service Principal create for the WhoIsWho Identity Backend |
| WHOISWHO_IDENTITY_FE_APPID | Store inside the appId of the Service Principal create for the WhoIsWho Identity Frontend |

4. Launch manually the GitHub action named 'Deploy WhoIsWho' with the following parameters:

|                            Parameter                                      | Value |
| --- | --- |
| Resource Group Name | The name of resource group where the WhoIsWho resources will be created|
| Resource Location | The resources location |
| Resources Name Main Identifier | The string that will identify uniquely all the Azure Resources that will be created, ex. if set to the value **'mywhoiswho'** deploy, the following resources will be created: app-**mywhoiswho**, appi-**mywhoiswho**, func-**mywhoiswho**-azureloader, func-**mywhoiswho**-datasync, plan-**mywhoiswho**, srch-**mywhoiswho**|

5. Wait that the deploument will be completed
