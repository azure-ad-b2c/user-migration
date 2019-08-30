
### Just in time migration flow
In case you don't have access to the passwords, you can migrate the user just-in-time, when a user sign-in for the first time. 

Just in time migration need to cuver follwoing flows:
* **Sign-in** when user sign-in, check if user fully migrated. If not, validate the user credential agains the legacy system. If valid, create the user using Graph API. If not valid, throw user fraindly error message "Invalid user name or password"
* **Sign-up** Check if user is existed in the legacy system (not migrated). Throw error message "there is a user with same email address".
* **Password resest** user may forget the password before the migration is copleted. In this case you need to migrate the user with the password provided by the end user.

You configure your Azure AD B2C policy to call a Restful API endpoint. Sending `SigninNames` and `password` claims, as input parameters. On sign-in, when a user provides the username and password, your endpoint, first checks if the user exists in the legacy identity provider.

If exists, validate the credential against the legacy identity provider. The validation is done by comparing the password in HASH formant, or calling a web service to run the validation against the legacy identity provider. 
* If the password provided by the user is valid, you update the user's Azure AD B2C account with that password. 
* Otherwise, if the password is not valid, you do nothing, just let B2C to throw __Bad user name or password__ error message. 

After the __just in time migration__, you need to change the user status (migrated), or remove the entity from your legacy identity provider.

> [!NOTE]
> This flow requires using Azure AD B2C [Custom policy](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-overview-custom)
>

Following diagram illustrates the just in time migration flow:
![Just in time migration flow](media/active-directory-b2c-user-migration-jit/just-in-time-migration.png)


## Disclaimer

The migration application is developed and managed by the open-source community in GitHub. The application is not part of Azure AD B2C product and it's not supported under any Microsoft standard support program or service. This migration app is provided AS IS without warranty of any kind. For any issue, visit the GitHub repository.