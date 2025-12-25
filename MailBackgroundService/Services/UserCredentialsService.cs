using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using MailBackgroundService.Services.Interfaces;
using System.IO;

namespace MailBackgroundService.Services
{
    public class UserCredentialsService : IUserCredentialsService
    {
        private string appname = "";
        private string email = "";
        private UserCredential credential;
        private bool initiated = false;

        public UserCredentialsService (IConfiguration configuration)
        {
            email = configuration["Email"];
            appname = configuration["AppName"];
            if (Directory.Exists(createPath()))
            {
                setCredentials();
            }
        }
        public void setCredentials()
        {
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    [GmailService.Scope.GmailReadonly],
                    "user",
                    CancellationToken.None,
                    new FileDataStore(createPath())).Result;

            }
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
