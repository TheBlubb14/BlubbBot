using BlubbBot.Autorization;
using Newtonsoft.Json;
using System.IO;

namespace BlubbBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var secret = JsonConvert.DeserializeObject<Secret>(File.ReadAllText(@"..\..\..\..\secret"));
            return;
            var webAuthorization = new WebAuthorization();
            var a = webAuthorization.GetAuthorizationCode(secret.client_id, secret.redirect_uri);
            var b = webAuthorization.GetAccessTokenAsync(a, secret.redirect_uri, secret.client_id, secret.client_secret).Result;
            var c = webAuthorization.RefreshAccessTokenAsync(b, secret.client_id, secret.client_secret).Result;
        }
    }
}
