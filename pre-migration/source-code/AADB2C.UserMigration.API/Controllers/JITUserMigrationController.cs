using AADB2C.GraphService;
using AADB2C.UserMigration.API.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AADB2C.UserMigration.API.Controllers
{
    public class JITUserMigrationController : ApiController
    {
        private readonly string Tenant = ConfigurationManager.AppSettings["b2c:Tenant"];
        private readonly string ClientId = ConfigurationManager.AppSettings["b2c:ClientId"];
        private readonly string ClientSecret = ConfigurationManager.AppSettings["b2c:ClientSecret"];

        [HttpPost]
        [Route("api/JITUserMigration/LoalAccountSignIn")]
        public async Task<IHttpActionResult> LoalAccountSignIn()
        {
            // If not data came in, then return
            if (this.Request.Content == null) throw new Exception();

            // Read the input claims from the request body
            string input = Request.Content.ReadAsStringAsync().Result;

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = JsonConvert.DeserializeObject(input, typeof(InputClaimsModel)) as InputClaimsModel;

            if (inputClaims == null)
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            TableUserEntity userMigrationEntity = UserMigrationService.RetrieveUser(inputClaims.email.ToLower());

            if (userMigrationEntity != null)
            {
                // Compare the password entered by the user and the one in the migration table
                if (ValidateCredentials(inputClaims.email, inputClaims.password))
                {
                    Trace.WriteLine($"User '{inputClaims.email}' exists in migration table, password is matched, the service is creating new AAD account");
                    B2CGraphClient b2CGraphClient = new B2CGraphClient(this.Tenant, this.ClientId, this.ClientSecret);
                    try
                    {
                        //TBD: Read user data from your old identity provider and set the values here
                        string DisplayName = "User disaply name";
                        string FirstName = "User first name";
                        string LastName = "User last name";

                        // Create the user
                        await b2CGraphClient.CreateAccount(
                            "emailAddress",
                            inputClaims.email,
                            null,
                            null,
                            null,
                            inputClaims.password,
                            DisplayName, 
                            FirstName, 
                            LastName,
                            false);

                        // Remove the user entity from migration table
                        UserMigrationService.RemoveUser(inputClaims.email.ToLower());

                        // Wait until user is created
                        await Task.Delay(1500);

                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                        return Content(HttpStatusCode.Conflict, new B2CResponseContent("Can not migrate user", HttpStatusCode.Conflict));
                    }
                }
                else
                {
                    Trace.WriteLine($"User '{inputClaims.email}' exists in migration table, passwords do not match");
                    return Content(HttpStatusCode.Conflict, new B2CResponseContent("Your password is incorrect (migraion API)", HttpStatusCode.Conflict));
                }
            }
            else
            {
                Trace.WriteLine($"No action required for user '{inputClaims.email}'");
            }
            return Ok();
        }

        /// <summary>
        /// TBD add you own business logic here
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool ValidateCredentials(string email, string password)
        {
            return (password == "1234");
        }

        [HttpPost]
        [Route("api/JITUserMigration/LoalAccountSignUp")]
        public IHttpActionResult LoalAccountSignUp()
        {
            // If not data came in, then return
            if (this.Request.Content == null) throw new Exception();

            // Read the input claims from the request body
            string input = Request.Content.ReadAsStringAsync().Result;

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = JsonConvert.DeserializeObject(input, typeof(InputClaimsModel)) as InputClaimsModel;

            if (inputClaims == null)
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            TableUserEntity userMigrationEntity = UserMigrationService.RetrieveUser(inputClaims.email.ToLower());

            if (userMigrationEntity != null)
            {
                Trace.WriteLine($"User '{inputClaims.email}' exists in migration table, prevent user from creating new one");
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("User name already in use (migraion API)", HttpStatusCode.Conflict));

            }
            return Ok();
        }

        [HttpPost]
        [Route("api/JITUserMigration/LoalAccountPasswordRest")]
        public async Task<IHttpActionResult> LoalAccountPasswordRest()
        {
            return null;
        }

    }
}
