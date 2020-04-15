using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AADB2C.BulkCreate
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
        }

        public static async Task CreateUserAsync(UserModel user)
        {
            try
            {
                await graphClient.Users
                .Request()
                .AddAsync(user);

            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Cannot create user {user.DisplayName}. Error: {ex.Message}");
            }
        }

        public static async Task<int> GetMeAsync()
        {
            try
            {
                var users = await graphClient.Users
                .Request()
                .GetAsync();

                // GET /me
                return users.Count;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return 0;
            }
        }
    }
}