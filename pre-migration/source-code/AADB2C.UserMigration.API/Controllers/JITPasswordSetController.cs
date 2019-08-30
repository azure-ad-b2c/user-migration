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
    public class JITPasswordSetController : ApiController
    {
        private readonly string Tenant = ConfigurationManager.AppSettings["b2c:Tenant"];
        private readonly string ClientId = ConfigurationManager.AppSettings["b2c:ClientId"];
        private readonly string ClientSecret = ConfigurationManager.AppSettings["b2c:ClientSecret"];

        [HttpPost]
        [Route("api/JITPasswordSet/LoalAccountSignIn")]
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
                    try
                    {

                        Trace.WriteLine($"User '{inputClaims.email}' exists in migration table, password is matched, the service is updating AAD account password");
                        B2CGraphClient b2CGraphClient = new B2CGraphClient(this.Tenant, this.ClientId, this.ClientSecret);

                        // Update user's password
                        await b2CGraphClient.UpdateUserPassword(inputClaims.email, inputClaims.password);

                        // Remove the user entity from migration table
                        UserMigrationService.RemoveUser(inputClaims.email.ToLower());

                        // Wait until password is set
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
    }
}
