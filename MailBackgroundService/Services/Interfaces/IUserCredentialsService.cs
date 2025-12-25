using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;

namespace MailBackgroundService.Services.Interfaces
{
    public interface IUserCredentialsService
    {
        public UserCredential getCredentials();
        public string getAppName();
        public AuthorizationCodeRequestUrl setCredentials();
        public void setToken(string code);
    }
}
