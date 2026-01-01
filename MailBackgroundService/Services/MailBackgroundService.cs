using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MailBackgroundService.Helpers;
using MailBackgroundService.Models;
using MailBackgroundService.Services.Interfaces;
using MimeKit;
using System.Text;

namespace MailBackgroundService.Services
{
    public class MailBackgroundService : BackgroundService
    {
        private readonly ILogger<MailBackgroundService> _logger;
        private readonly IGoogleUserCredentialsService _singletonService;
        private readonly I3PLUserCredentialsService _3PLUserCredentialsService;
        private readonly IRatesService _ratesService;
        private bool send = true;
        public MailBackgroundService(ILogger<MailBackgroundService> logger, IGoogleUserCredentialsService singletonService, I3PLUserCredentialsService _3PLUserCredentialsService, IRatesService ratesService)
        {
            _logger = logger;
            _singletonService = singletonService;
            this._3PLUserCredentialsService = _3PLUserCredentialsService;
            _ratesService = ratesService; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"ExecuteAsync {DateTime.UtcNow}");
            try
            {
                _3PLUserCredentialsService.GetToken();

                while (!stoppingToken.IsCancellationRequested)
                {
                    UserCredential credential = _singletonService.GetCredentials();
                    string appName = _singletonService.GetAppName();
                    if (credential != null)
                    {
                        var ads = _ratesService.GetRates(new RatesInput("Hello!\r\n\r\nWe have an order shipping from TSG - 08837 to the address listed below:\r\n\r\nMohammed Alshmayri\r\n24612 Dunning Street Dearborn Michigan 48124 US\r\n(313)213-1695\r\nmohamedalshmayri@gmail.com\r\n\r\nThe dimensions are as follows:\r\n\r\n1 Double Pallet-  16pcs - 104x44x41 1000lbs\r\n\r\nReference number: T1323302 / 31536023\r\n\r\nThese are ready to assemble. Notify Prior to Arrival, Lift Gate, Residential.\r\n\r\n2. \r\n\r\nHello!\r\n\r\nWe have an order shipping from Fabuwood -07105 to the address listed below:\r\n"), true);
                        _singletonService.SetCredentials();
                        var service = new GmailService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = appName,
                        });
                        // 3. Request messages
                        //var listRequest = service.Users.Messages.List(email);
                        string query = "from:@gmail.com";
                        var asd = Map.Instace.List;
                        // Request List of Messages
                        var listRequest = service.Users.Messages.List("me");
                        listRequest.Q = query;


                        //if (File.Exists(tokenFolder))
                        //{
                            
                        //}
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
                                            Console.WriteLine(decodedBody);
                                        }
                                    }
                                }
                                else if (message.Payload.Body != null)
                                {
                                    // Handle Singlepart messages
                                    string decodedBody = DecodeBase64Url(message.Payload.Body.Data);
                                    Console.WriteLine(decodedBody);
                                }
                            }
                        }

                        if (send)
                        {
                            send = false;
                            var emailMessage = new MimeMessage();
                            emailMessage.From.Add(new MailboxAddress("Sender Name", "cveleserbia@gmail.com"));
                            emailMessage.To.Add(new MailboxAddress("", "cvelenn@gmail.com"));
                            emailMessage.Subject = "test subject";
                            emailMessage.Body = new TextPart("plain") { Text = "test body" };

                            var message = new Message
                            {
                                Raw = Base64UrlEncode(emailMessage.ToString())
                            };

                            // 4. Send
                            //await service.Users.Messages.Send(message, "me").ExecuteAsync();
                        }
                    }
                    await Task.Delay(5000, stoppingToken); // Wait 5 seconds
                }


            }
            catch (Exception ex)
            {

                throw;
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
