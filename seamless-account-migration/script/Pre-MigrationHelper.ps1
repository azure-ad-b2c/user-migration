#Part 1 - Obtain an Access Token to Azure AD Graph API
#AAD B2C tenant
$tenant = "b2cprod.onmicrosoft.com"
#B2CUserMigration Application Registration Application Id
$ClientID      = "" 
#B2CUserMigration Application Registration generated key (client secret)  
$ClientSecret  = ""     
$loginURL = "https://login.microsoftonline.com"
$resource = "https://graph.microsoft.com"

# Get an OAuth 2 access token based on client id, secret and tenant
$body = @{grant_type="client_credentials";client_id=$ClientID;client_secret=$ClientSecret;resource=$resource}
$oauth = Invoke-RestMethod -Method Post -Uri $loginURL/$tenant/oauth2/token?api-version=1.0 -Body $body

#Part 2 - Register the extension attribute named "requiresMigration" into Azure AD B2C
#ObjectID of the b2c-extensions-app App Registration
$AppObjectID = ""

#Set the endpoint to register extension attributes
$url = "$resource/beta/applications/$AppObjectID/extensionProperties"

#Define the extension attribute
$body = @"
{ 
 "name": "requiresMigration", 
 "dataType": "Boolean", 
 "targetObjects": ["User"]
}
"@

#Generate the authentication header and make the request
$authHeader = @{"Authorization"= $oauth.access_token;"Content-Type"="application/json";"ContentLength"=$body.length }
$result = Invoke-WebRequest -Headers $authHeader -Uri $url -Method Post -Body $body

#Print the full attribute Name
($result.Content | Convertfrom-Json).name
$extName = ($result.Content | Convertfrom-Json).name
$extName

#Part 3 - Create a user object in Azure AD B2C
#Populate the user properties
#Example $extName = extension_ce0f3b39c19d415988af620a33887208_requiresMigration
$body = @"
{
  "displayName": "John Smith",
  "identities": [
    {
      "signInType": "emailAddress",
      "issuer": "b2cprod.onmicrosoft.com",
      "issuerAssignedId": "jsmith@yahoo.com"
    }
  ],
  "passwordProfile" : {
    "password": "Password123!!",
    "forceChangePasswordNextSignIn": false
  },
  "passwordPolicies": "DisablePasswordExpiration",
  "$extName": true
}
"@

#Build the authentication header
$authHeader = @{"Authorization"= $oauth.access_token;"Content-Type"="application/json";"ContentLength"=$body.length }

#Set the endpoint to make the POST request to
$url = "$resource/beta/users"

#Make the POST request with the body to create the user
Invoke-WebRequest -Headers $authHeader -Uri $url -Method Post -Body $body