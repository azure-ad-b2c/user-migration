# Azure Active Directory B2C: Just in time user migration
When you plan to migrate your identity provider to Azure AD B2C, you may also need to migrate the users account as well. This article explains how to migrate existing user accounts with their passwords and profiles, from any identity provider database to Azure AD B2C.

Just in time migration flow fits when the user's password is not accessible. For example:
* Passwords are in HASH format
* Passwords are stored in an identity provider, which you don't have access. Your system validates user credential by calling an identity provider web service

## Disclaimer
The migration application is developed and managed by the open-source community in GitHub. The application is not part of Azure AD B2C product and it's not supported under any Microsoft standard support program or service. 
This migration app is provided AS IS without warranty of any kind.

## Just in time migration flow
Just in time migration deals with following scenarios:
* **Sign-in** Configure your Azure AD B2C policy to call a Restful API endpoint. 
    * Sending `SigninNames` and `password` claims, as input parameters. 
    * The REST API, first checks if the user exists in the legacy identity provider. 
    * If exists, checks if the password provided by the user is valid (compare password or call the legacy identity provider). 
    * If valid the REST API creates the account (using Graph API) and delete the migration entity. Otherwise, the REST API return an error 'Your password is incorrect'.

* **Sign-up**  Configure your Azure AD B2C policy to call a Restful API endpoint. 
    * Sending `SigninNames`claims, as input parameters. 
    * The REST API checks if the use exists in the migration table. 
    * If exists, return an error message 'A user with the specified ID already exists. Please choose a different one.'

* **Password reset** 
    * Fist page where user provides and verify the email address - Configure your Azure AD B2C policy to call a Restful API endpoint. 
        * Sending `SigninNames`claims, as input parameters. 
        * The REST API validates if the user exists in Azure AD or in the migration table. Otherwise return error message 'An account could not be found for the provided user ID.' 
    * Second page where user provides the new password. 
        * REST API creates the account with the new password provided by the end user and deletes the migration entity.
        * For migrated users and non-migrated users, the REST API returns the user objectId


## Unit tests
You should run at least following tests:
1. **Sign-in with migration account** - Check that the user is created in Azure AD B2C and also the user entity is removed from the migration table. To make sure the migration passed successfully, sign-in again with the same email address and password. 
1. **Sign-in with new user you created** - Check that migration process is not running and you can sign-in successfully
1. **Sign-in with account that you know it's not exsis** - Check that you get an error message (We can't seem to find your account)
1. **Sign-up with migration account** -  You should get an error message that this account already exists (from migration API)
1. **Sign-up with account you created** - You should get an error message that this account already exists (from Azure AD B2C)
1. **Sign-up with account that you know it's not existed** - Make sure you can create new account
1. **Reset the password for migration account** - Check that the user is created in Azure AD B2C and also the user entity is removed from the migration table. To make sure the migration passed successfully, sign-in again with the same email address and password.
1. **Reset the password for account you created** - Sign-in again and make sure you the password is set correctly
1. **Reset the password for account you know is not exist** - Make sure you can the error account is not existed
 
## Just in time migration .Net core solution

### Migration database
This demo web API illustrates a case when username and password (in HASH format) are stored in Azure Tables. Azure table serves as your legacy identity provider. You can change to code to work directly with your legacy identity provider. Or export the data to Azure Table.

Populate the table with dummy data. Call following endpoint (replace URL to fits your deployment):
`http://your-app.azurewebsites.net/api/test/PopulateMigrationTable`

> Note: You can customize the Restful service to accommodate your own business logic. For example, validate user credential against SQL database, No-SQL or calling remote web services. The idea is the same: if the password provided by user is valid, update the AAD account and remove the entity.

### Application Settings
To test the demo Restful API. Open the `AADB2C.JITUserMigration.sln` Visual Studio solution in Visual Studio. In the `AADB2C.JITUserMigration` project, open the `appsettings.json`. Replace the app settings with your own values:
```JSON
"AppSettings": {
    "Tenant": "<Your Azure AD B2C tenant name>",
    "ClientId": "<Your Azure AD Graph app ID>",
    "ClientSecret": "<Your Azure AD Graph app secret>",
    "BlobStorageConnectionString": "<Your connection string to Azure Table that stores your identities to be migrated>"

  }
```


## Run the solution
To run the visual studio solution, you need:
1. Create Azure AD application and set read permissions. For more information, see [Azure Active Directory B2C: User migration](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-user-migration) section 1.1, 1.2 and 1.3. Note: you don't need write permissions.
2. Deploy this web app to Azure App Services. For more information, see [Create and publish the web app](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-get-started-dotnet#create-and-publish-the-web-app)
3. Set the application settings. You can set the app settings directly from `launchSettings.json` file. Or use the better solution, from Azure portal. For more information, see: [Configure web apps in Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/web-sites-configure#application-settings)
4. Open the policies files, change the tenant name, client_id and IdTokenAudience for Local Account SignIn, and upload the policies to Azure portal.

### Important notes:
Secure the communication between Azure AD B2C to your Rest API. For more information, see: [Secure your RESTful service by using client certificates](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-custom-rest-api-netfw-secure-cert) OR [Secure your RESTful services by using HTTP basic authentication](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-custom-rest-api-netfw-secure-basic)

## Solution artifacts
### Azure AD custom policy

This sample policy is based on [LocalAccounts starter pack](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/tree/master/SocialAndLocalAccounts). 
   * All changes are marked with **Demo:** comment inside the policy XML files.
   * Make the necessary changes in the **Action required** comments

### Visual studio solution
* **UserMigrationController** The custom policy calls this REST API
* **TestController** populate the Azure Table with migration dummy data.
* **appsettings.json** application settings
* **AzureADGraphClient.cs** Azure AD Graph client
* **Models** folder - this folder contains the necessary object-mapping classes 