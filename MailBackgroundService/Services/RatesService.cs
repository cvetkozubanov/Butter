using MailBackgroundService.Models;
using MailBackgroundService.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace MailBackgroundService.Services
{
    public class RatesService : IRatesService
    {
        private readonly ILogger<GoogleUserCredentialsService> _logger;
        private readonly I3PLUserCredentialsService _3PLUserCredentialsService;
        private readonly HttpClient client = new HttpClient();
        private string authURI = "";
        public RatesService(IConfiguration configuration, ILogger<GoogleUserCredentialsService> logger, I3PLUserCredentialsService i3PLUserCredentialsService) 
        {
            _logger = logger;
            authURI = configuration["RatesURI"];
            _3PLUserCredentialsService = i3PLUserCredentialsService;
        }
        public RatesOutput[] GetRates(RatesInput input, bool firstTime)
        {
            try
            {
                var token = _3PLUserCredentialsService.AccessToken;

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never // Ensures nulls are sent
                };
                HttpResponseMessage response = client.PostAsJsonAsync(authURI, input, options).Result;

                string responseBody = response.Content.ReadAsStringAsync().Result;
                RatesOutput[] ratesOutput = JsonConvert.DeserializeObject<List<RatesOutput>>(responseBody).ToArray();
                return ratesOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError("RatesService GetRates", ex.Message);
                if (firstTime)
                {
                    _3PLUserCredentialsService.GetToken();
                    return GetRates(input, false);
                }
            }
            return null;

        }
    }
}
