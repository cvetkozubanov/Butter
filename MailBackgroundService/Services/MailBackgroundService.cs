using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MailBackgroundService.Models;
using MailBackgroundService.Services.Interfaces;
using MimeKit;
using Scriban;
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
        private readonly string customerId = "";
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
            customerId = configuration["CustomerId"];
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

                        string query = $"from:{readFromEmail} is:unread";
                        var listRequest = service.Users.Messages.List("me");
                        listRequest.Q = query;

                        var response = listRequest.Execute();
                        _logger.LogInformation($"Execute - no of messages: {response.Messages.Count}");

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

                    } else 
                    {
                        _logger.LogInformation($"ExecuteAsync credentials not found");
                    }
                    await Task.Delay(60000, stoppingToken); // Wait 5 seconds
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
                _logger.LogInformation($"ParseAndRespondAsync {JsonSerializer.Serialize(rateInput)}");

                var rates = _ratesService.GetRates(rateInput, true);
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sender Name", readFromEmail));
                emailMessage.To.Add(new MailboxAddress("", email));

                string templateContent = File.ReadAllText("Templates/rate.html");
                var template = Template.Parse(templateContent);
                var result = template.Render(new { rates = rates.Select(r => new { Value = r.TransitTime }) });

                var textPart= new TextPart("html") { Text = result };
                textPart.ContentType.Charset = "utf-8";
                emailMessage.Body = textPart;

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
                    _logger.LogInformation($"Reply to email");
                    await service.Users.Messages.Send(messageResponse, "me").ExecuteAsync();
                }
                rateInput.CustomerId = customerId;
                _logger.LogInformation($"Quote rate");
                _ratesService.QuoteRate(rateInput, true);
                //createShipmentAPI
                ModifyMessageRequest mods = new ModifyMessageRequest();
                mods.RemoveLabelIds = new List<string> { "UNREAD" };
                _logger.LogInformation($"Mark as read");
                service.Users.Messages.Modify(mods, "me", m.Id).Execute();
            }
            else
            {
                _logger.LogError($"Unable to parse mail: {decodedBody}");
            }

        }
        private string Base64UrlEncode(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
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
