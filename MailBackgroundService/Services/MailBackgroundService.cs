using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MailBackgroundService.Models;
using MailBackgroundService.Services.Interfaces;
using MimeKit;
using System.Text;
using System.Text.Json;

namespace MailBackgroundService.Services
{
    public class MailBackgroundService : BackgroundService
    {
        private readonly ILogger<MailBackgroundService> _logger;
        private readonly IGoogleUserCredentialsService _singletonService;
        private readonly I3PLUserCredentialsService _3PLUserCredentialsService;
        private readonly I3PLGlobalUserCredentialsService _3PLGlobalUserCredentialsService;
        private readonly IRatesService _ratesService;
        private readonly string readFromEmail = "";
        private readonly string email = "";
        private readonly string createShipmentAPI = "";
        public MailBackgroundService(IConfiguration configuration, ILogger<MailBackgroundService> logger, IGoogleUserCredentialsService singletonService, I3PLUserCredentialsService _3PLUserCredentialsService, I3PLGlobalUserCredentialsService _3PLGlobalUserCredentialsService, IRatesService ratesService)
        {
            _logger = logger;
            _singletonService = singletonService;
            this._3PLUserCredentialsService = _3PLUserCredentialsService;
            this._3PLGlobalUserCredentialsService = _3PLGlobalUserCredentialsService;
            _ratesService = ratesService;
            readFromEmail = configuration["ReadFromEmail"];
            email = configuration["Email"];
            createShipmentAPI = configuration["CreateShipmentAPI"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"ExecuteAsync {DateTime.UtcNow}");
            try
            {
                _3PLUserCredentialsService.GetToken();
                _3PLGlobalUserCredentialsService.GetToken();

                while (!stoppingToken.IsCancellationRequested)
                {
                    UserCredential credential = _singletonService.GetCredentials();
                    string appName = _singletonService.GetAppName();
                    if (credential != null)
                    {
                        _singletonService.SetCredentials();
                        var service = new GmailService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = appName,
                        });

                        string query = $"from:{readFromEmail}";
                        var listRequest = service.Users.Messages.List("me");
                        listRequest.Q = query;

                        var response = listRequest.Execute();
                        _logger.LogInformation($"Execute {response.Messages.Count}");

                        if (response.Messages != null)
                        {
                            foreach (var m in response.Messages)
                            {
                                Message message = service.Users.Messages.Get("me", m.Id).Execute();
                                if (message.Payload.Parts != null)
                                {
                                    // Handle Multipart messages
                                    foreach (var part in message.Payload.Parts)
                                    {
                                        if (part.MimeType == "text/plain" || part.MimeType == "text/html")
                                        {
                                            string base64UrlData = part.Body.Data;
                                            string decodedBody = DecodeBase64Url(base64UrlData);
                                            await ParseAndRespondAsync(decodedBody, m, service);
                                            //"Hello!\r\n\r\nWe have an order shipping from TSG - 08837 to the address listed below:\r\n\r\nMohammed Alshmayri\r\n24612 Dunning Street Dearborn Michigan 48124 US\r\n(313)213-1695\r\nmohamedalshmayri@gmail.com\r\n\r\nThe dimensions are as follows:\r\n\r\n1 Double Pallet-  16pcs - 104x44x41 1000lbs\r\n\r\nReference number: T1323302 / 31536023\r\n\r\nThese are ready to assemble. Notify Prior to Arrival, Lift Gate, Residential.\r\n\r\n2. \r\n\r\nHello!\r\n\r\nWe have an order shipping from Fabuwood -07105 to the address listed below:\r\n"), true);

                                            Console.WriteLine(decodedBody);
                                        }
                                    }
                                }
                                else if (message.Payload.Body != null)
                                {
                                    // Handle Singlepart messages
                                    string decodedBody = DecodeBase64Url(message.Payload.Body.Data);
                                    await ParseAndRespondAsync(decodedBody, m, service);
                                    Console.WriteLine(decodedBody);
                                }
                            }
                        }

                    }
                    await Task.Delay(5000, stoppingToken); // Wait 5 seconds
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

        }

        private async Task ParseAndRespondAsync(string decodedBody, Message m, GmailService service) 
        {
            var rateInput = new RatesInput(decodedBody);
            if (rateInput.Valid)
            {
                var rates = _ratesService.GetRates(rateInput, true);
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sender Name", readFromEmail));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Body = new TextPart("plain") { Text = $"Best rates are: \r\n\r\n {string.Join("\r\n\r\n", rates.Select(r => JsonSerializer.Serialize(r)))}" };

                // Essential headers for threading
                emailMessage.Headers.Add("In-Reply-To", m.Id);
                emailMessage.Headers.Add("References", m.Id);


                var messageResponse = new Message
                {
                    ThreadId = m.Id,
                    Raw = Base64UrlEncode(emailMessage.ToString())
                };
                if (rates.Length > 0)
                {
                    await service.Users.Messages.Send(messageResponse, "me").ExecuteAsync();
                }
                //createShipmentAPI
            }
            else
            {
                _logger.LogError($"Unable to parse mail: {decodedBody}");
            }

        }
        private string Base64UrlEncode(string input)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        private string DecodeBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0 ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("-", "+").Replace("_", "/");
            byte[] data = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(data);
        }
    }

}
