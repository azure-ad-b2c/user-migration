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
    public class TestController : Controller
    {
        private readonly AppSettingsModel AppSettings;

        // Demo: Inject an instance of an AppSettingsModel class into the constructor of the consuming class, 
        // and let dependency injection handle the rest
        public TestController(IOptions<AppSettingsModel> appSettings)
        {
            this.AppSettings = appSettings.Value;
        }

        [HttpGet(Name = "PopulateMigrationTable")]
        public async Task<ActionResult> PopulateMigrationTable()
        {

            CloudTable table = await UserMigrationController.GetSignUpTable(this.AppSettings.BlobStorageConnectionString);

            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Create a customer entity and add it to the table.
            List<UserTableEntity> identities = new List<UserTableEntity>();
            identities.Add(new UserTableEntity("Jeff@contoso.com", "1234", "Jeff", "Smith"));
            identities.Add(new UserTableEntity("Ben@contoso.com", "1234", "Ben", "Smith"));
            identities.Add(new UserTableEntity("Linda@contoso.com", "1234", "Linda", "Brown"));
            identities.Add(new UserTableEntity("Sarah@contoso.com", "1234", "Sarah", "Miller"));
            identities.Add(new UserTableEntity("William@contoso.com", "1234", "William", "Johnson"));
            identities.Add(new UserTableEntity("John@contoso.com", "1234", "John", "Miller"));
            identities.Add(new UserTableEntity("Emily@contoso.com", "1234", "Emily", "Miller"));
            identities.Add(new UserTableEntity("David@contoso.com", "1234", "David", "Johnson"));
            identities.Add(new UserTableEntity("Amanda@contoso.com", "1234", "Amanda", "Davis"));
            identities.Add(new UserTableEntity("Brian@contoso.com", "1234", "Brian", "Wilson"));
            identities.Add(new UserTableEntity("Anna@contoso.com", "1234", "Anna", "Miller"));

            // Add both customer entities to the batch insert operation.
            foreach (var item in identities)
            {
                batchOperation.InsertOrReplace(item);
            }

            // Execute the batch operation.
            await table.ExecuteBatchAsync(batchOperation);

            return Ok(identities);
        }
    }
}
