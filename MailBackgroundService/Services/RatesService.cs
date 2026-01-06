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
        private readonly I3PLGlobalUserCredentialsService _3PLGlobalUserCredentialsService;
        private readonly HttpClient client = new HttpClient();
        private string authURI = "";
        private string createShipmentAPI = "";
        public RatesService(IConfiguration configuration, ILogger<GoogleUserCredentialsService> logger, I3PLUserCredentialsService i3PLUserCredentialsService, I3PLGlobalUserCredentialsService _3PLGlobalUserCredentialsService) 
        {
            _logger = logger;
            authURI = configuration["RatesURI"];
            createShipmentAPI = configuration["CreateShipmentAPI"];
            _3PLUserCredentialsService = i3PLUserCredentialsService;
            this._3PLGlobalUserCredentialsService = _3PLGlobalUserCredentialsService;
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
                var ratesOutput = JsonConvert.DeserializeObject<List<RatesOutput>>(responseBody).ToList().OrderBy(x => x.Billed);
                return ratesOutput.Take(2).ToArray();
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

        public void QuoteRate(RatesInput input, bool firstTime)
        {
            try
            {
                var token = _3PLGlobalUserCredentialsService.AccessToken;

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never // Ensures nulls are sent
                };
                HttpResponseMessage response = client.PostAsJsonAsync(createShipmentAPI, input, options).Result;

                string responseBody = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                _logger.LogError("RatesService QuoteRate", ex.Message);
                if (firstTime)
                {
                    _3PLGlobalUserCredentialsService.GetToken();
                    QuoteRate(input, false);
                }
            }
        }
    }
}
