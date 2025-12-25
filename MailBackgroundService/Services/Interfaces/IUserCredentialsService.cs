using Google.Apis.Auth.OAuth2;

namespace MailBackgroundService.Services.Interfaces
{
    public interface IUserCredentialsService
    {
        public UserCredential getCredentials();
        public string getAppName();
        public void setCredentials();
    }
}
