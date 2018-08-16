using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TwitchLib.Api.Enums;

namespace BlubbBot.Autorization
{
    internal class WebAuthorization
    {
        public const string accounts_base_url = "https://id.twitch.tv/oauth2";

        private static readonly HttpClient httpClient;

        static WebAuthorization()
        {
            httpClient = new HttpClient();
        }

        /// <summary>
        /// 1. GetAuthorizationCode
        /// </summary>
        /// <param name="client_id">Client ID</param>
        /// <param name="redirect_uri">Redirect URL</param>
        /// <param name="force_verify"></param>
        /// <param name="scope"></param>
        /// <returns>authorization_code</returns>
        public string GetAuthorizationCode(string client_id, string redirect_uri, bool force_verify = false, params AuthScopes[] scope)
        {
            // Some string to prevent users from Cross-Site-Request-Forgery(CSRF/XSRF)
            var csrftoken = Guid.NewGuid().ToString();

            var uri = new Uri($"{accounts_base_url}/authorize")
                .AddParameter("client_id", client_id)
                .AddParameter("redirect_uri", redirect_uri.TrimEnd('/'))
                .AddParameter("response_type", "code")
                .AddParameter("scope", string.Join(" ", scope).Trim())
                .AddParameter("state", csrftoken)
                .AddParameter("force_verify", force_verify.ToString().ToLower());

            // see https://github.com/dotnet/corefx/issues/10361
            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
            Uri result;

            using (var server = new Webserver(redirect_uri))
            {
                result = server.WaitListen();
            }

            var values = HttpUtility.ParseQueryString(result.Query);

            if (values["state"] != csrftoken)
            {
                throw new Exception($"retrieved state: '{values[csrftoken]}' is not '{csrftoken}'");
            }

            return values["code"];
        }

        /// <summary>
        /// 2. GetAccessToken
        /// </summary>
        /// <param name="authorization_code">Authorization Code from <see cref="GetAuthorizationCode"/></param>
        /// <param name="redirect_uri">The same Redirect URI which was used in <see cref="GetAuthorizationCode"/></param>
        /// <param name="client_id">Client ID</param>
        /// <param name="client_secret">Client Secret</param>
        /// <returns><see cref="AccessToken"/></returns>
        public async Task<AccessToken> GetAccessTokenAsync(string authorization_code, string redirect_uri, string client_id, string client_secret)
        {
            var url = $"{accounts_base_url}/token";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("client_secret", client_secret),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorization_code),
                new KeyValuePair<string, string>("redirect_uri", redirect_uri.Trim('/'))
            });

            var responseMessage = "";

            var response = await httpClient.PostAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var result = JsonConvert.DeserializeObject<AccessToken>(responseMessage);
            this.CalculateExpiration(ref result);

            return result;
        }

        /// <summary>
        /// 3. RefreshAccessToken
        /// </summary>
        /// <param name="accessToken">Access Token</param>
        /// <param name="client_id">Client ID</param>
        /// <param name="client_secret">Client Secret</param>
        /// <returns><see cref="AccessToken"/></returns>
        public async Task<AccessToken> RefreshAccessTokenAsync(AccessToken accessToken, string client_id, string client_secret)
        {
            var url = $"{accounts_base_url}/token";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("client_secret", client_secret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", accessToken.refresh_token),
            });

            var responseMessage = "";

            var response = await httpClient.PostAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            accessToken = JsonConvert.DeserializeObject<AccessToken>(responseMessage);

            this.CalculateExpiration(ref accessToken);

            return accessToken;
        }

        private void CalculateExpiration(ref AccessToken accessToken)
        {
            accessToken.expires_at = DateTime.Now.AddSeconds(accessToken.expires_in);
        }
    }
}
