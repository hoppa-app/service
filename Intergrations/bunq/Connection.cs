using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Hosting;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;
using hoppa.Service.Data;
using hoppa.Service.Model;

using Bunq.Sdk.Context;

namespace hoppa.Service.Intergrations.bunq
{
    public class Connnection : hoppa.Service.Model.Connection
    {
        public Connnection(IHostingEnvironment env, JObject tokens)
        {
            Type = "bunq";
            Parameters = new Dictionary<string, object>
            {
                {"bunqContext", GetContext(env, (string)tokens["access_token"]) }
            };

            // Get User Details
            BunqContext.LoadApiContext(ApiContext.FromJson((string)Parameters["bunqContext"]));

            ExternalId = BunqContext.UserContext.UserId;
        }
        public Connnection(IHostingEnvironment env, string apiKey)
        {
            Type = "bunq";
            Parameters = new Dictionary<string, object>
            {
                {"bunqContext", GetContext(env, apiKey) }
            };

            // Get User Details
            BunqContext.LoadApiContext(ApiContext.FromJson((string)Parameters["bunqContext"]));

            ExternalId = BunqContext.UserContext.UserId;
        }
        public static JObject GetTokens(string code)
        {
            HttpClient client = new HttpClient();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("X-Bunq-Client-Request-Id", System.Guid.NewGuid().ToString())
            };

            var httpContent = new FormUrlEncodedContent(pairs);

            string url = String.Format(
                "https://api-oauth.sandbox.bunq.com/v1/token?grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}",
                code,
                Configuration.Current.Service.Intergrations.bunq.RedirectUri,
                Configuration.Current.Service.Intergrations.bunq.ClientId,
                Configuration.Current.Service.Intergrations.bunq.ClientSecret
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

        public static string GetContext(IHostingEnvironment env, string apiKey)
        {
            if (env.IsDevelopment())
            {
                var currentIpGet = new HttpClient().GetStringAsync("http://ipinfo.io/ip");
                string currentIp = Regex.Replace(currentIpGet.Result.ToString(), @"\t|\n|\r", "");
                List<string> DevelopmentIPs = new List<string>{
                    "52.166.78.97",
                    "52.166.76.57",
                    "52.166.72.50",
                    "52.166.72.42",
                    "52.166.73.59",
                    currentIp
                };
                var apiContextSetup = ApiContext.Create(ApiEnvironmentType.SANDBOX, apiKey, "hoppa", DevelopmentIPs);
                return apiContextSetup.ToJson();
            }
            else
            {
                List<string> ServicePlanIPs = new List<string>{
                    "52.166.78.97",
                    "52.166.76.57",
                    "52.166.72.50",
                    "52.166.72.42",
                    "52.166.73.59"
                };

                var apiContextSetup = ApiContext.Create(ApiEnvironmentType.PRODUCTION, apiKey, "hoppa", ServicePlanIPs);
                return apiContextSetup.ToJson();
            }
        }

        public static void ValidateStatus(
            IHostingEnvironment _hostingEnvironment, 
            string userGuid, 
            string connectionGuid, 
            string requestId
        )
        {
            var client = new HttpClient();
            int count = 0;
            
            
            client.DefaultRequestHeaders.Add("X-Bunq-Client-Request-Id", "unique");

            var result = JObject.Parse((client.GetAsync("https://api.tinker.bunq.com/v1/credential-password-ip-request/" + requestId).Result).Content.ReadAsStringAsync().Result);
            
            string status = (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["status"];

            while(status == "CREATED" && count < 100)
            {   
                count += 1;
                Console.WriteLine("Status is: Created\n Poging: " + count);
                Thread.Sleep(5000);
                
                result = JObject.Parse((client.GetAsync("https://api.tinker.bunq.com/v1/credential-password-ip-request/" + requestId).Result).Content.ReadAsStringAsync().Result);
                status = (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["status"];
            }
            if(status == "ACCEPTED")
            {
                Console.WriteLine("Status is: Accepted");

                Connection connection = new Intergrations.bunq.Connnection(_hostingEnvironment, (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["api_key"]);
                connection.Guid = System.Guid.NewGuid().ToString();

                PersonRepository _personRepository = new PersonRepository();
                var person = _personRepository.GetPerson(userGuid).Result;

                if(person.Connections == null)
                {
                    person.Connections = new List<Connection>();
                }

                person.Connections.Add(connection);
            
                var update = _personRepository.UpdatePerson(userGuid, person).Result;
                }
            else
            {
                Console.WriteLine("Error with: " + requestId);
            }
        }
    }
}