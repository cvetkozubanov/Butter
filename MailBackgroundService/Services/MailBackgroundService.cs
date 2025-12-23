using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailBackgroundService.Managers;
using System.Net;

namespace MailBackgroundService.Services
{
    public class MailBackgroundService : BackgroundService
    {
        private readonly ILogger<MailBackgroundService> _logger;

        public MailBackgroundService(ILogger<MailBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string[] Scopes = { GmailService.Scope.GmailReadonly };
            // Load client secrets (credentials.json)
            try
            {
                GoogleCredential credential;
                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // Use the required scopes for your API
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(Scopes);
                }

                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Butter",
                });

                // Keep running until the application stops
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Service running at: {time}", DateTimeOffset.Now);
                    //var listRequest = service.Users.Labels.List("me");
                    //var labels = listRequest.Execute();

                    //if (labels.Labels != null)
                    //{
                    //    foreach (var label in labels.Labels)
                    //    {
                    //        _logger.LogInformation("Label: {labelName}", label.Name);
                    //    }
                    //}
                    // Wait 5 seconds before the next iteration
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}
