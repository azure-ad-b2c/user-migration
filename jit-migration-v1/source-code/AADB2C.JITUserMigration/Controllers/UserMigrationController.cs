using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AADB2C.JITUserMigration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AADB2C.JITUserMigration.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UserMigrationController : Controller
    {
        private readonly AppSettingsModel AppSettings;

        // Demo: Inject an instance of an AppSettingsModel class into the constructor of the consuming class, 
        // and let dependency injection handle the rest
        public UserMigrationController(IOptions<AppSettingsModel> appSettings)
        {
            this.AppSettings = appSettings.Value;
        }

        [HttpPost(Name = "LoalAccountSignIn")]
        public async Task<ActionResult> LoalAccountSignIn()
        {
            string input = null;

            // If not data came in, then return
            if (this.Request.Body == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is null", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = InputClaimsModel.Parse(input);

            if (inputClaims == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            if (string.IsNullOrEmpty(inputClaims.signInName))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("User 'signInName' is null or empty", HttpStatusCode.Conflict));
            }

            if (string.IsNullOrEmpty(inputClaims.password))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Password is null or empty", HttpStatusCode.Conflict));
            }

            // Create a retrieve operation that takes a customer entity.
            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            var retrieveOperation = TableOperation.Retrieve<UserTableEntity>(Consts.MigrationTablePartition, inputClaims.signInName.ToLower());

            CloudTable table = await GetSignUpTable(this.AppSettings.BlobStorageConnectionString);

            // Execute the retrieve operation.
            TableResult userMigrationEntity = await table.ExecuteAsync(retrieveOperation);

            if (userMigrationEntity != null && userMigrationEntity.Result != null)
            {
                try
                {
                    // Compare the password entered by the user and the one in the migration table
                    if (inputClaims.password == ((UserTableEntity)userMigrationEntity.Result).Password)
                    {
                        try
                        {
                            await MigrateUser(inputClaims, table, userMigrationEntity);

                            // Wait until user is created
                            await Task.Delay(1500);

                        }
                        catch (Exception ex)
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Can not migrate user", HttpStatusCode.Conflict));

                        }
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Your password is incorrect (migraion API)", HttpStatusCode.Conflict));
                    }
                    return Ok();
                }
                catch (Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"User migration error: {ex.Message}", HttpStatusCode.Conflict));
                }
            }

            return Ok();
        }


        [HttpPost(Name = "LoalAccountSignUp")]
        public async Task<ActionResult> LoalAccountSignUp()
        {
            string input = null;

            // If not data came in, then return
            if (this.Request.Body == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is null", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = InputClaimsModel.Parse(input);

            if (inputClaims == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            if (string.IsNullOrEmpty(inputClaims.signInName))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("User 'signInName' is null or empty", HttpStatusCode.Conflict));
            }

            // Create a retrieve operation that takes a customer entity.
            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            var retrieveOperation = TableOperation.Retrieve<UserTableEntity>(Consts.MigrationTablePartition, inputClaims.signInName.ToLower());

            CloudTable table = await GetSignUpTable(this.AppSettings.BlobStorageConnectionString);

            // Execute the retrieve operation.
            TableResult userMigrationEntity = await table.ExecuteAsync(retrieveOperation);

            if (userMigrationEntity != null && userMigrationEntity.Result != null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("A user with the specified ID already exists. Please choose a different one. (migraion API)", HttpStatusCode.Conflict));
            }

            return Ok();
        }


        [HttpPost(Name = "LoalAccountUserExsist")]
        public async Task<ActionResult> LoalAccountUserExsist()
        {
            string input = null;

            // If not data came in, then return
            if (this.Request.Body == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is null", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = InputClaimsModel.Parse(input);

            if (inputClaims == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            if (string.IsNullOrEmpty(inputClaims.signInName))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("User 'signInName' is null or empty", HttpStatusCode.Conflict));
            }


            // Create a retrieve operation that takes a customer entity.
            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            var retrieveOperation = TableOperation.Retrieve<UserTableEntity>(Consts.MigrationTablePartition, inputClaims.signInName.ToLower());

            CloudTable table = await GetSignUpTable(this.AppSettings.BlobStorageConnectionString);

            // Execute the retrieve operation.
            TableResult userMigrationEntity = await table.ExecuteAsync(retrieveOperation);

            if (userMigrationEntity != null && userMigrationEntity.Result != null)
            {
                return Ok();
            }

            AzureADGraphClient azureADGraphClient = new AzureADGraphClient(this.AppSettings.Tenant, this.AppSettings.ClientId, this.AppSettings.ClientSecret);

            GraphAccountModel account = await azureADGraphClient.SearcUserBySignInNames(inputClaims.signInName);

            if (account == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"An account could not be found for the provided user ID. (user migration)", HttpStatusCode.Conflict));
            }

            return Ok();

        }

        [HttpPost(Name = "LoalAccountPasswordReset")]
        public async Task<ActionResult> LoalAccountPasswordReset()
        {
            string input = null;

            // If not data came in, then return
            if (this.Request.Body == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is null", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = InputClaimsModel.Parse(input);

            if (inputClaims == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            if (string.IsNullOrEmpty(inputClaims.signInName))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("User 'signInName' is null or empty", HttpStatusCode.Conflict));
            }

            if (string.IsNullOrEmpty(inputClaims.signInName))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Password is null or empty", HttpStatusCode.Conflict));
            }

            // Create a retrieve operation that takes a customer entity.
            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            var retrieveOperation = TableOperation.Retrieve<UserTableEntity>(Consts.MigrationTablePartition, inputClaims.signInName.ToLower());

            CloudTable table = await GetSignUpTable(this.AppSettings.BlobStorageConnectionString);

            // Execute the retrieve operation.
            TableResult userMigrationEntity = await table.ExecuteAsync(retrieveOperation);

            if (userMigrationEntity != null && userMigrationEntity.Result != null)
            {
                try
                {
                    try
                    {
                        await MigrateUser(inputClaims, table, userMigrationEntity);

                        // Wait until user is created
                        await Task.Delay(3000);

                    }
                    catch (Exception ex)
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Can not migrate user", HttpStatusCode.Conflict));

                    }
                }
                catch (Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"User migration error: {ex.Message}", HttpStatusCode.Conflict));
                }


            }

            AzureADGraphClient azureADGraphClient = new AzureADGraphClient(this.AppSettings.Tenant, this.AppSettings.ClientId, this.AppSettings.ClientSecret);

            GraphAccountModel account = await azureADGraphClient.SearcUserBySignInNames(inputClaims.signInName);

            if (account == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"An account could not be found for the provided user ID. (user migration)", HttpStatusCode.Conflict));
            }

            OutputClaimsModel output = new OutputClaimsModel();
            output.objectId = account.objectId;

            return Ok(output);

        }
        public static async Task<CloudTable> GetSignUpTable(string conectionString)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(conectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(Consts.MigrationTable);

            // Create the table if it doesn't exist.
            await table.CreateIfNotExistsAsync();

            return table;
        }

        private async Task MigrateUser(InputClaimsModel inputClaims, CloudTable table, TableResult userMigrationEntity)
        {
            AzureADGraphClient azureADGraphClient = new AzureADGraphClient(this.AppSettings.Tenant, this.AppSettings.ClientId, this.AppSettings.ClientSecret);

            // Create the user using Graph API
            await azureADGraphClient.CreateAccount(
                "emailAddress",
                inputClaims.signInName,
                null,
                null,
                null,
                inputClaims.password,
                ((UserTableEntity)userMigrationEntity.Result).DisplayName,
                ((UserTableEntity)userMigrationEntity.Result).FirstName,
                ((UserTableEntity)userMigrationEntity.Result).LastName);

            // Remove the user entity from migration table
            TableOperation deleteOperation = TableOperation.Delete((UserTableEntity)userMigrationEntity.Result);
            await table.ExecuteAsync(deleteOperation);
        }
    }
}
