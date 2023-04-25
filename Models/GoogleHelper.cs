using System.Text.Json;
using System.Text;

namespace BuzCalendarV2.Models
{
    public class GoogleHelper
    {
        public static string BuildConsentURL(string clientId, string[] scope,string response_type,string access_type,string include_granted_scopes, string prompt, string redirectUri = null)
        {
            return $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&scope={string.Join(" ", scope)}&response_type=code&access_type={access_type}&include_granted_scopes={include_granted_scopes}&prompt={prompt}";
        }

        private static string BuildAuthorizationCodeRequest(string code, string clientId, string secret,
            string redirectUri)
        {
            return
                $"code={code}&client_id={clientId}&client_secret={secret}&redirect_uri={redirectUri}&grant_type=authorization_code";
        }

        private static string BuildRefreshAccessTokenRequest(string refreshToken, string clientId, string secret)
        {
            return
                $"client_id={clientId}&client_secret={secret}&refresh_token={refreshToken}&grant_type=refresh_token";
        }


        private static async Task<GoogleAuthResponse> PostMessage(string postData)
        {
            GoogleAuthResponse result;

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
            var request = new HttpRequestMessage(HttpMethod.Post, "token");
            request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.SendAsync(request);
            using (var content = response.Content)
            {
                var json = content.ReadAsStringAsync().Result;
                result = JsonSerializer.Deserialize<GoogleAuthResponse>(json);
            }
            return result;
        }


        public static async Task<GoogleAuthResponse> ExchangeAuthorizationCode(string code, string clientId, string secret,
            string redirectUri = null)
        {
            var result = new GoogleAuthResponse();

            var postData = BuildAuthorizationCodeRequest(code, clientId, secret, redirectUri);

            return await PostMessage(postData);
        }

        public static async Task<GoogleAuthResponse> ExchangeRefreshToken(string refreshToken, string clientId, string secret)
        {
            var postData = BuildRefreshAccessTokenRequest(refreshToken, clientId, secret);

            return await PostMessage(postData);
        }
    }
}
