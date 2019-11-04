using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using Microsoft.Graph.Auth;

namespace AADB2C.BulkCreate
{
    class Program
    {



        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private static async Task RunAsync()
        {
            AppSettings config = AppSettings.ReadFromJsonFile();

            // Initialize the auth provider with values from appsettings.json
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(config.AppId)
                .WithTenantId(config.TenantId)
                .WithClientSecret(config.AppSecret)
                .Build();

            //ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Temporary using Graph API (HTTP)
            var result = await confidentialClientApplication
                .AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync(); ;

            // Get the users to import
            string appDirecotyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dataFilePath = Path.Combine(appDirecotyPath, config.UsersFileName);

            // Check file existence 
            if (!File.Exists(dataFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File '{dataFilePath}' not found");
                Console.ResetColor();
                return;
            }

            // Read the data file and convert to object
            UsersModel users = UsersModel.Parse(File.ReadAllText(dataFilePath));


            // Initialize Graph client
            //GraphHelper graphHelper = new GraphHelper();
            //GraphHelper.Initialize(authProvider);

            // Temporary using Graph API (HTTP)
            GraphAPIClient graphHelper = new GraphAPIClient(result.AccessToken, "beta");
            // Create users
            foreach (UserModel item in users.Users)
            {
                item.SetB2CProfile(config.TenantId);
                await graphHelper.CreateUserAsync(item);
            }


            Console.ReadLine();
        }
    }
}
