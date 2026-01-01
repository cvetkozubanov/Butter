namespace MailBackgroundService.Services.Interfaces
{
    public interface I3PLUserCredentialsService
    {
        public string GetToken();
        public string AccessToken { get; }
    }
}
