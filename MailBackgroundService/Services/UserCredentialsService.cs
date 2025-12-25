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
    public class UserCredentialsService : IUserCredentialsService
    {
        private string appname = "";
        private string email = "";
        private string redirectURL = "";
        private UserCredential credential;
        private GoogleAuthorizationCodeFlow flow;
        private readonly ILogger<UserCredentialsService> _logger;
        public UserCredentialsService (IConfiguration configuration, ILogger<UserCredentialsService> logger)
        {
            email = configuration["Email"];
            appname = configuration["AppName"];
            redirectURL = configuration["RedirectURL"];
            _logger = logger;
            if (Directory.Exists(createPath()))
            {
                //credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                //    new ClientSecrets
                //    {
                //        ClientId = "1038892956158-g1pegsbfth319um403517vvdvuu2tid6.apps.googleusercontent.com",
                //        ClientSecret = "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"
                //    },
                //    new[] { GmailService.Scope.GmailReadonly },
                //    "user", // Unique identifier for the user
                //    CancellationToken.None,
                //    new FileDataStore(createPath()) // This saves the token to %appdata%
                //).Result;
                flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = "1038892956158-g1pegsbfth319um403517vvdvuu2tid6.apps.googleusercontent.com",
                        ClientSecret = "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"
                    },
                    Scopes = new[] { GmailService.Scope.GmailReadonly },
                    // FileDataStore is commonly used
                    DataStore = new FileDataStore(createPath())
                });

                // Load the credential
                string userId = "user_id";
                credential = new UserCredential(flow, "user_id", flow.LoadTokenAsync(userId, CancellationToken.None).Result);
            }
        }
        public AuthorizationCodeRequestUrl setCredentials()
        {
            //using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            //{
            //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //        GoogleClientSecrets.Load(stream).Secrets,
            //        [GmailService.Scope.GmailReadonly],
            //        "user",
            //        CancellationToken.None,
            //        new FileDataStore(createPath())).Result;
            //}


            //credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //    GoogleClientSecrets.FromFile("credentials.json").Secrets,
            //    [GmailService.Scope.GmailReadonly],
            //    "user", // Unique ID for the user
            //    CancellationToken.None,
            //    new FileDataStore(createPath()) // Stores token locally
            //).Result;

            flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "1038892956158-g1pegsbfth319um403517vvdvuu2tid6.apps.googleusercontent.com",
                    ClientSecret = "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"
                },
                Scopes = new[] { GmailService.Scope.GmailReadonly },
                DataStore = new FileDataStore(createPath()), // Persistent store
                Prompt = "consent",
            });
            string state = Guid.NewGuid().ToString("N");
            var codeRequest = flow.CreateAuthorizationCodeRequest(redirectURL);
            codeRequest.State = state;
            //codeRequest.Prompt = "consent"; // Forces consent screen
            // 1. Redirect user to Google to consent
            // 2. Capture the code from the redirect_uri
            // 3. Exchange the code for a token
            return codeRequest;
        }

        public void setToken(string code)
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

        string IUserCredentialsService.getAppName()
        {
            return appname;
        }

        UserCredential IUserCredentialsService.getCredentials()
        {
            return credential;
        }
    }
}
