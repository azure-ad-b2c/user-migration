# User migration
When you plan to migrate your identity provider to Azure AD B2C, you may also need to migrate the users account as well. Following examples demonstrate how to migrate existing user accounts with their passwords and profiles, from any identity provider to Azure AD B2C.

## Pre migration

This flow applies when you either have clear access to a user's credentials (user name and password) or the credentials are encrypted, but you can decrypt them. The pre-migration process involves reading the users from the old identity provider and creating new accounts in the Azure AD B2C directory. Read more about [user migration](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-user-migration)


## Just in time migration
Just in time migration flow fits when the user's password is not accessible. For example: Passwords are in HASH format. Or when passwords are stored in an identity provider, which you don't have access. Your system validates user credential by calling an identity provider web service.

- [just in time migration v1](jit-migration-v1) - In this sample Azure AD B2C call a REST API that validates the credential, and migrate the account with a Graph API call.

- [just in time migration v2](jit-migration-v2) - In this sample Azure AD B2C call a REST API to validate the credentials, return the user profile to B2C, while B2C creates the amount in the directory.


> **Important!** Just in time migration uses a custom REST API to validate the user's credentials in the legacy identity provider. Make sure your REST API is protected. For example, with **brute-force attack**, an attacker submitting many passwords with the hope of eventually guess the user credentials. On the REST API side, you should lock the account and preventing such attacks.

## Disclaimer
The migration application is developed and managed by the open-source community in GitHub. The application is not part of Azure AD B2C product and it's not supported under any Microsoft standard support program or service. This migration app is provided AS IS without warranty of any kind.
