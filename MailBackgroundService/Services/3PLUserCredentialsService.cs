using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using MailBackgroundService.Services.Interfaces;
using System;
using MailBackgroundService.Models;
using System.Text.Json;

namespace MailBackgroundService.Services
{
    public class _3PLUserCredentialsService : I3PLUserCredentialsService
    {
        private readonly ILogger<GoogleUserCredentialsService> _logger;
        private string ClientId = "";
        private string ClientSecret = "";
        private string authURI = "";
        public string AccessToken = "";
        public _3PLUserCredentialsService(IConfiguration configuration, ILogger<GoogleUserCredentialsService> logger)
        {
            authURI = configuration["AuthURI"];
            ClientId = configuration["ClientId3PL"];
            ClientSecret = configuration["ClientSecret3PL"];
            _logger = logger;
        }
        private readonly HttpClient client = new HttpClient();

        string I3PLUserCredentialsService.AccessToken => AccessToken;

        public string GetToken()
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret)
            };

            var content = new FormUrlEncodedContent(data);

            HttpResponseMessage response = client.PostAsync(authURI, content).Result;

            var res = response.Content.ReadAsStringAsync().Result;
            Token token = JsonSerializer.Deserialize<Token>(res);
            AccessToken = token.access_token;
            return token.access_token;
            
        }
    }
}
