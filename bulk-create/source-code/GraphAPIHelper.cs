
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.BulkCreate
{
    public class GraphAPIClient
    {
        public readonly string GraphEndpoint = "https://graph.microsoft.com/";
        public readonly string GraphVersion = "beta";

        public string AccessToken { get; }

        public GraphAPIClient(string accessToken, string graphApiVersion)
        {
            this.AccessToken = accessToken;
            this.GraphVersion = graphApiVersion;
        }

        public string BuildUrl(string api, string query)
        {
            string url = $"{this.GraphEndpoint}{this.GraphVersion}/{api}";

            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\r\nGraph URL: {url}");
            Console.ResetColor();

            return url;
        }

        public async Task<string> CreateUserAsync(UserModel user)
        {
            string data = user.ToString();
            return await SendGraphRequest("users", null, data, HttpMethod.Post);
        }

        /// <summary>
        /// Handle Graph user API, support following HTTP methods: GET, POST and PATCH
        /// </summary>
        public async Task<string> SendGraphRequest(string api, string query, string data, HttpMethod method)
        {
            // Set the Graph url. Including: Graph-endpoint/tenat/users?api-version&query
            string url = BuildUrl(api, query);

            return await SendGraphRequest(method, url, data);
        }


        public async Task<string> SendGraphRequest(HttpMethod method, string url, string data)
        {
            try
            {
                using (HttpClient http = new HttpClient())
                using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                {
                    // Set the authorization header
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AccessToken);

                    // For POST and PATCH set the request content 
                    if (!string.IsNullOrEmpty(data))
                    {
                        //Trace.WriteLine($"Graph API data: {data}");
                        request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                    }

                    // Send the request to Graph API endpoint
                    using (HttpResponseMessage response = await http.SendAsync(request))
                    {
                        string error = await response.Content.ReadAsStringAsync();

                        // Check the result for error
                        if (!response.IsSuccessStatusCode)
                        {
                            // Throw server busy error message
                            if (response.StatusCode == (HttpStatusCode)429)
                            {
                                // TBD: Add you error handling here
                            }

                            throw new Exception(error);
                        }

                        // Return the response body, usually in JSON format
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception)
            {
                // TBD: Add you error handling here
                throw;
            }
        }
    }
}