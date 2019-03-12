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

        [HttpPost(Name = "migrate")]
        public async Task<ActionResult> Migrate()
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

            //if (string.IsNullOrEmpty(inputClaims.password))
            //{
            //    return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Password is null or empty", HttpStatusCode.Conflict));
            //}

            // Initiate the output claim object
            B2CResponseModel outputClaims = new B2CResponseModel("", HttpStatusCode.OK);

            // Create a retrieve operation that takes a customer entity.
            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            var retrieveOperation = TableOperation.Retrieve<UserTableEntity>(Consts.MigrationTablePartition, inputClaims.signInName.ToLower());

            CloudTable table = await GetSignUpTable(this.AppSettings.BlobStorageConnectionString);

            // Execute the retrieve operation.
            TableResult tableEntity = await table.ExecuteAsync(retrieveOperation);
            

            if (tableEntity != null && tableEntity.Result != null)
            {
                UserTableEntity userMigrationEntity = ((UserTableEntity)tableEntity.Result);
                try
                {
                    outputClaims.needToMigrate = "local";

                    // Compare the password entered by the user and the one in the migration table.
                    // Don't compare in password reset flow (useInputPassword is true)
                    if (inputClaims.useInputPassword || (inputClaims.password == userMigrationEntity.Password))
                    {
                        outputClaims.newPassword = inputClaims.password;
                        outputClaims.email = inputClaims.signInName;
                        outputClaims.displayName = userMigrationEntity.DisplayName;
                        outputClaims.surName = userMigrationEntity.LastName;
                        outputClaims.givenName = userMigrationEntity.FirstName;

                        // Remove the user entity from migration table
                        TableOperation deleteOperation = TableOperation.Delete((UserTableEntity)tableEntity.Result);
                        await table.ExecuteAsync(deleteOperation);
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("Your password is incorrect (migration API)", HttpStatusCode.Conflict));
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"User migration error: {ex.Message}", HttpStatusCode.Conflict));
                }
            }

            return Ok(outputClaims);
        }


        [HttpPost(Name = "RaiseErrorIfExists")]
        public async Task<ActionResult> RaiseErrorIfExists()
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
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("A user with the specified ID already exists. Please choose a different one. (migration API)", HttpStatusCode.Conflict));
            }

            return Ok();
        }

        [HttpPost(Name = "RaiseErrorIfNotExists")]
        public async Task<ActionResult> RaiseErrorIfNotExists()
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

            // Checks if user exists in Azure AD B2C or the migration table. If not, raises an error
            if ((userMigrationEntity != null && userMigrationEntity.Result != null) || (string.IsNullOrEmpty(inputClaims.objectId) == false))
            {
                return Ok();
            }
            else
            { 
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("An account could not be found for the provided user ID. (migration API)", HttpStatusCode.Conflict));
            }
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
    }
}
