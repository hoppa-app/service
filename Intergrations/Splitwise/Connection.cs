using System;
using System.Text;
using System.Collections.Generic;

using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;

namespace hoppa.Service.Intergrations.Splitwise
{
    public class Connnection : hoppa.Service.Model.Connection
    {
        public Connnection(JObject tokens)
        {
            Type = "splitwise";
            Parameters = new Dictionary<string, object>
            {
                {"AccessToken", (string)tokens["access_token"] }
            };
            
            // Get User Details
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("https://secure.splitwise.com/api/v3.0/")
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)Parameters["AccessToken"]);

            var connectedUser = JObject.Parse((client.GetAsync("get_current_user").Result).Content.ReadAsStringAsync().Result)["user"];

            ExternalId = (int)connectedUser["id"];
            DisplayName = (string)connectedUser["first_name"] + " " + connectedUser["last_name"];
            UserName = (string)connectedUser["email"];
        }
        public static JObject GetTokens(string code)
        {
            HttpClient client = new HttpClient();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", Configuration.Current.Service.Intergrations.Splitwise.ClientId),
                new KeyValuePair<string, string>("client_secret", Configuration.Current.Service.Intergrations.Splitwise.ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", Configuration.Current.Service.Intergrations.Splitwise.RedirectUri),
                new KeyValuePair<string, string>("code", code)
            };
            
            var httpContent = new FormUrlEncodedContent(pairs);

            string url = String.Format(
                "https://secure.splitwise.com/oauth/token"
            );
            
            try
            {
                return JObject.Parse((client.PostAsync(url, httpContent).Result).Content.ReadAsStringAsync().Result);
            }
            catch
            {
                return null;
            }
        }
    }
}