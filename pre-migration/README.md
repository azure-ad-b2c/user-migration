# Azure-AD-B2C-UserMigration

When you plan to migrate your identity provider to Azure AD B2C, you may also need to migrate the users account as well. This code sample and Azure AD B2C policy demonstrate how to migrate existing user accounts, from any identity provider to Azure AD B2C. This code sample is not meant to be prescriptive, but rather describes two of several different approaches. The developer is responsible for suitability and performances.
For more information how to:
* To migrate local accounts, see [User migration](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-user-migration)
* To migrate social identities, see [Migrate users with social identities](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-social-migration)

## User migration flows
Azure AD B2C allows you to migrate the uses through [Graph API](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-devquickstarts-graph-dotnet). User migration process falls into two flows:

* **One time migration** - This flow fits when you have access to user credentials (user name and password) in a clear text. Or the credentials are encrypted, but you are able to decrypt. The process involves: reading the users from your legacy system and create new accounts in Azure AD B2C directory.

* **Just in time migration** - This flow fits when the user's password is not accessible. For example:
    * Passwords are in HASH format
    * Passwords are stored in an identity provider, which you don't have access. Your system validates user credential by calling an identity provider web service.
    * For more information, see:  [jsut-in-time user migration](just-in-time-migration.ms).

## Password policy
The Azure AD B2C password policy for local accounts is based on the policy for Azure AD. Azure AD B2C's sign-up or sign-in and password reset policies uses the "strong" password strength and doesn't expire any passwords. Read the [Azure AD password policy](https://msdn.microsoft.com/library/azure/jj943764.aspx) for more details.

If the accounts that you want to migrate from an existing user store has lower password strength than the [strong password strength enforced by Azure AD B2C](https://msdn.microsoft.com/library/azure/jj943764.aspx), you can disable the strong password requirement using the `DisableStrongPassword` value in the `passwordPolicies` property. For instance, you can modify the create user request as follows: `"passwordPolicies": "DisablePasswordExpiration, DisableStrongPassword"`.

> Note: This application already configuered with `DisableStrongPassword` 

## Configure the application settings
* **b2c:Tenant** Your Tenant Name
* **b2c:ClientId** The ApplicationID from above
* **b2c:ClientSecret** The Client Secret Key from above
* **MigrationFile** Name of a JSON file containing the users' data; for example, UsersData.json
* **BlobStorageConnectionString** Your connection Azure table string

## Commands
* **Migrate users with password**, run `UserMigration.exe 1` command.
* **Migrate users with random password**, run `UserMigration.exe 2` command. This operation also creates an Azure table entity. Later, you configure the policy to call the REST API service. The service uses an Azure table to track and manage the migration process.
* **Read user data by email address**, run `UserMigration.exe 3 {email address}` command
* **Read user data by display name**, run `UserMigration.exe 4 {display name}` command
* **Clean up your Azure AD tenant** and remove all users listed in the JSON file, run the `UserMigration.exe 5`


## User migration import data file
This application uses a JSON file that contains dummy user data, including: local account, social account, and local & social identities in single account.  To edit the JSON file, open the `AADB2C.UserMigration.sln` Visual Studio solution. In the `AADB2C.UserMigration` project, open the `UsersData.json` file. The file contains a list of user entities. Each user entity has the following properties:
* **signInName** - For local account, e-mail address to sign-in
* **displayName** - User display name
* **firstName** - User's first name
* **lastName** - User's last name
* **password** For local account, user's password (can be empty)
* **issuer** - For social account, the identity provider name
* **issuerUserId** - For social account, the unique user identifier used by the social identity provider. The value should be in clear text. The sample app encodes this value to  based64.
* **email** For social account only (not combined), user's email address

```JSON
{
  "userType": "emailAddress",
  "Users": [
    {
      // Local account only
      "signInName": "James@contoso.com",
      "displayName": "James Martin",
      "firstName": "James",
      "lastName": "Martin",
      "password": "Pass!w0rd"
    },
    {
      // Social account only
      "issuer": "Facebook.com",
      "issuerUserId": "1234567890",
      "email": "sara@contoso.com",
      "displayName": "Sara Bell",
      "firstName": "Sara",
      "lastName": "Bell"
    },
    {
      // Combine local account with social identity
      "signInName": "david@contoso.com",
      "issuer": "Facebook.com",
      "issuerUserId": "0987654321",
      "displayName": "David Hor",
      "firstName": "David",
      "lastName": "Hor",
      "password": "Pass!w0rd"
    }
  ]
}
```

> **Note**: If you don't update the UsersData.json file with your data. After the migration process is finished, you can sign-in with the local account credentials. The social accounts are dummy, and can't be used. To migrate your social account, provider a real data.

## Register your application in your tenant
To communicate with the Graph API, you first must have a service account with administrative privileges. In Azure AD, you register an application and authentication to Azure AD. The application credentials are **Application ID** and **Application Secret**. The application acts as itself, not as a user, to call the Graph API.
Follow the instructions [Step 1: Use Azure AD Graph API to migrate users](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-user-migration#step-1-use-azure-ad-graph-api-to-migrate-users) how to:
1. Register your application in your tenant
1. Create the application secret
1. Grant administrative permission to your application
1. (Optional) Environment cleanup

## Disclaimer
The migration application is developed and managed by the open-source community in GitHub. The application is not part of Azure AD B2C product and it's not supported under any Microsoft standard support program or service. 
This migration app is provided AS IS without warranty of any kind. For any issue, visit the [GitHub repository](https://github.com/yoelhor/Azure-AD-B2C-UserMigration).
