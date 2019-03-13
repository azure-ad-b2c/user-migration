# User migration
When you plan to migrate your identity provider to Azure AD B2C, you may also need to migrate the users account as well. Following examples demonstrate how to migrate existing user accounts with their passwords and profiles, from any identity provider to Azure AD B2C.

## Pre migration

This flow applies when you either have clear access to a user's credentials (user name and password) or the credentials are encrypted, but you can decrypt them. The pre-migration process involves reading the users from the old identity provider and creating new accounts in the Azure AD B2C directory. Read more about [user migration](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-user-migration)


## Just in time migration
Just in time migration flow fits when the user's password is not accessible. For example: Passwords are in HASH format. Or when passwords are stored in an identity provider, which you don't have access. Your system validates user credential by calling an identity provider web service.

- [just in time migration v1](jit-migration-v1) - In this sample Azure AD B2C calls a REST API that validates the credential, and migrate the account with a Graph API call.

- [just in time migration v2](jit-migration-v2) - In this sample Azure AD B2C calls a REST API to validate the credentials, return the user profile to B2C from an Azure Table, and B2C creates the account in the directory.

- [seamless-account-migration](seamless-account-migration) - Where accounts have been pre-migrated into Azure AD B2C and you want to update the password on the account on initial sign in. Azure AD B2C calls a REST API to validate the credentials for accounts marked as requiring migration (via attribute) against a legacy identity provider, returns a successful response to Azure AD B2C, and Azure AD B2C writes the password to the account in the directory.

> **Important!** Just in time and seamless migration approaches use a custom REST API to validate the user's credentials against the legacy identity provider. Make sure your REST API is protected against **brute-force attacks**. An attacker may submit many passwords with the hope of eventually guessing the users credentials. On the REST API side, you should stop serving requests for the account to prevent such attacks.

## Disclaimer
The migration application is developed and managed by the open-source community in GitHub. The application is not part of Azure AD B2C product and it's not supported under any Microsoft standard support program or service. This migration app is provided AS IS without warranty of any kind.
