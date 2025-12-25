using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailBackgroundService.Helpers;
using MailBackgroundService.Managers;
using MailBackgroundService.Services.Interfaces;
using System;
using System.Net;
using System.Text;

namespace MailBackgroundService.Services
{
    public class MailBackgroundService : BackgroundService
    {
        private readonly ILogger<MailBackgroundService> _logger;
        private readonly IUserCredentialsService _singletonService;
        public MailBackgroundService(ILogger<MailBackgroundService> logger, IUserCredentialsService singletonService)
        {
            _logger = logger;
            _singletonService = singletonService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string[] Scopes = {  GmailService.Scope.GmailReadonly };

            try
            { 
                while (!stoppingToken.IsCancellationRequested)
                {
                    UserCredential credential = _singletonService.getCredentials();
                    string appName = _singletonService.getAppName();
                    if (credential != null)
                    {
                        _singletonService.setCredentials();
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

                        var response = listRequest.Execute();
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
                    }
                    await Task.Delay(5000, stoppingToken); // Wait 5 seconds
                }


            }
            catch (Exception ex)
            {

                throw;
            }

        }
        string DecodeBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0 ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("-", "+").Replace("_", "/");
            byte[] data = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(data);
        }
    }

}
