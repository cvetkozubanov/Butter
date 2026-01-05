namespace MailBackgroundService.Services.Interfaces
{
    public interface I3PLGlobalUserCredentialsService
    {
        public string GetToken();
        public string AccessToken { get; }
    }
}
