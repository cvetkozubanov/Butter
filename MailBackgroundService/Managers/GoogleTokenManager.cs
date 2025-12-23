using Google.Apis.Auth.OAuth2;

namespace MailBackgroundService.Managers
{
    public class GoogleTokenManager
    {
        public static async Task<string> GetAccessTokenAsync()
        {
            // 1. Load the service account credentials from the JSON file
            GoogleCredential credential = GoogleCredential.FromAccessToken("AIzaSyCzHbyAYRIWyqo2JGJuzhU8B2GjWzGtF60");

            // 2. Define the scope(s) required (e.g., Cloud Storage, BigQuery)
            string[] scopes = { "www.googleapis.com" };
            credential = credential.CreateScoped(scopes);

            // 3. Request the access token
            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return token;
        }
    }
}
