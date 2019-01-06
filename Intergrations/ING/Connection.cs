using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;

namespace hoppa.Service.Intergrations.ING
{
    public class Connnection : hoppa.Service.Model.Connection
    {
        public Connnection(JObject tokens)
        {
            Type = "ing";
            Parameters = new Dictionary<string, object>
            {
                {"AccessToken" , (string)tokens["access_token"]},
                {"Expiration", Support.ConvertTimestamp(DateTime.UtcNow.AddMinutes(15))},
                {"RefreshToken", (string)tokens["refresh_token"] },
                {"ConsentedOn", Support.ConvertTimestamp(DateTime.UtcNow)}
            };
        }
        public static JObject GetTokens(string code)
        {
            List<KeyValuePair<string, string>> parameters = null;
            JObject response = null;

            // Get Server AccessToken
            parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "create_order granting payment-requests payment-requests:view payment-requests:create payment-requests:close virtual-ledger-accounts:fund-reservation:create virtual-ledger-accounts:fund-reservation:delete virtual-ledger-accounts:balance:view"),
            };

            response = Client.Post("/oauth2/token", parameters);
            string ServerAccessToken = (string)response["access_token"];

            // Get Client AccessToken
            parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", "xxx"),
            };

            response = Client.Post("/oauth2/token", parameters, ServerAccessToken);
            return response;
        }
    }
}