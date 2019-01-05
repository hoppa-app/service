using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;

namespace hoppa.Service.Intergrations.Rabobank
{
    public class Connnection : hoppa.Service.Model.Connection
    {
        public Connnection(JObject tokens)
        {
            Type = "rabobank";
            Parameters = new Dictionary<string, object>
            {
                {"AccessToken" , (string)tokens["access_token"]},
                {"Expiration", Support.ConvertTimestamp(DateTime.UtcNow.AddHours(1))},
                {"RefreshToken", (string)tokens["refresh_token"] },
                {"ConsentedOn", (double)tokens["consented_on"] }
            };
        }
        public static JObject GetTokens(string code)
        {
            HttpClient client = new HttpClient();
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue
            (
                "Basic", Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        String.Format(
                            "{0}:{1}", 
                            Configuration.Current.Service.Intergrations.Rabobank.ClientId,
                            Configuration.Current.Service.Intergrations.Rabobank.ClientSecret
                        )
                    )
                )
            );
                        
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", Configuration.Current.Service.Intergrations.Rabobank.RedirectUri),
                new KeyValuePair<string, string>("code", code)
            };
                
            var httpContent = new FormUrlEncodedContent(pairs);

            string url = String.Format(
                Configuration.Current.Service.Intergrations.Rabobank.ApiUri + "/oauth2/token"
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