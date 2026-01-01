using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using MailBackgroundService.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.Xml;

namespace MailBackgroundService.Services
{
    public class GoogleUserCredentialsService : IGoogleUserCredentialsService
    {
        private string appname = "";
        private string email = "";
        private string redirectURL = "";
        private UserCredential credential;
        private GoogleAuthorizationCodeFlow flow;
        private readonly ILogger<GoogleUserCredentialsService> _logger;
        private string ClientId = "";
        private string ClientSecret = "";
        public static string[] Scopes = new string[] { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailSend };
        public GoogleUserCredentialsService (IConfiguration configuration, ILogger<GoogleUserCredentialsService> logger)
        {
            email = configuration["Email"];
            appname = configuration["AppName"];
            redirectURL = configuration["RedirectURL"];
            ClientId = configuration["ClientId"];
            ClientSecret = configuration["ClientSecret"];
            _logger = logger;
            if (Directory.Exists(createPath()))
            {
                flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = ClientId,
                        ClientSecret = ClientSecret
                    },
                    Scopes = GoogleUserCredentialsService.Scopes,
                    DataStore = new FileDataStore(createPath())
                });

                string userId = "user_id";
                credential = new UserCredential(flow, "user_id", flow.LoadTokenAsync(userId, CancellationToken.None).Result);
            }
        }
        public AuthorizationCodeRequestUrl SetCredentials()
        {
            flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret
                },
                Scopes = GoogleUserCredentialsService.Scopes,
                DataStore = new FileDataStore(createPath()), // Persistent store
                Prompt = "consent",
            });
            string state = Guid.NewGuid().ToString("N");
            var codeRequest = flow.CreateAuthorizationCodeRequest(redirectURL);
            codeRequest.State = state;
            return codeRequest;
        }
        public bool IsKeyPresent()
        {
            return Directory.Exists(createPath()) && Directory.EnumerateFileSystemEntries(createPath()).Any();
        }
        public void SetToken(string code)
        {
            _logger.LogInformation("set token " + code + " " + flow + " " + redirectURL);

            var token = flow.ExchangeCodeForTokenAsync(
                "user_id",
                code,
                redirectURL,
                CancellationToken.None).Result;

            credential = new UserCredential(flow, "user_id", token);
        }

        private string createPath()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string tokenFolder = Path.Combine(exePath, $"MyNewGmailFolder-{email}-{appname}");
            return tokenFolder;
        }

        string IGoogleUserCredentialsService.GetAppName()
        {
            return appname;
        }

        UserCredential IGoogleUserCredentialsService.GetCredentials()
        {
            return credential;
        }
    }
}
