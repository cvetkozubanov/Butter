using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;

namespace MailBackgroundService.Services.Interfaces
{
    public interface IGoogleUserCredentialsService
    {
        public UserCredential GetCredentials();
        public string GetAppName();
        public AuthorizationCodeRequestUrl SetCredentials();
        public void SetToken(string code);
        public bool IsKeyPresent();

    }
}
