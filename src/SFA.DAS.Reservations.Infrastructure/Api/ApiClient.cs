using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public class ApiClient : IApiClient
    {
        private static readonly HttpClient Client = new HttpClient();

        public async Task<string> GetReservations()
        {
            var accessToken = await GetAccessTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync("https://localhost:44351/api/accounts/1/reservations").ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private static async Task<string> GetAccessTokenAsync()
        {
            var clientCredential = new ClientCredential("clientid", "clientsecret");
            var context = new AuthenticationContext("https://login.microsoftonline.com/citizenazuresfabisgov.onmicrosoft.com", true);

            AuthenticationResult result;
            try
            {
                
                result = await context.AcquireTokenAsync("https://citizenazuresfabisgov.onmicrosoft.com/das-reservations-api-at", clientCredential).ConfigureAwait(false);
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                var deviceCodeResult = await context.AcquireDeviceCodeAsync("https://citizenazuresfabisgov.onmicrosoft.com/das-reservations-api-at", "client id");
                Console.WriteLine(deviceCodeResult.Message);
                result = await context.AcquireTokenByDeviceCodeAsync(deviceCodeResult);
            }

            return result.AccessToken;
        }
    }
}