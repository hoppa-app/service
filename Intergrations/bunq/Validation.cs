using System;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;
using hoppa.Service.Core;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;

using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;

using System.Collections.Generic;

namespace hoppa.Service.Intergrations.bunq
{
    public class Validate
    {
        public static void Run(
            IPersonRepository _personRepository, 
            IHostingEnvironment _hostingEnvironment, 
            IOptions<Configuration> _settings, 
            string userGuid, 
            string connectionGuid, 
            string requestId
        )
        {
            var client = new HttpClient();
            string accessToken = null;
            int count = 0;

            Connection connection = new Connection();
            connection.Guid = Guid.NewGuid().ToString();
            
            var person = (_personRepository.GetPerson(userGuid)).Result;
            
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
                accessToken = (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["api_key"];
                
                // Register and set bunqContext
                string apiConfig = (new Context(_hostingEnvironment, accessToken)).bunqContext;
                connection.Type = "bunq";
                connection.Parameters = new Dictionary<string, object>
                {
                    {"bunqContext", apiConfig }
                };
                    
                // Gather connection details
                var apiContext = ApiContext.FromJson(apiConfig);
                BunqContext.LoadApiContext(apiContext);

                connection.ExternalId = BunqContext.UserContext.UserId;

                if(person.Connections == null)
                {
                    person.Connections = new List<Connection>();
                }

                person.Connections.Add(connection);
            
                _personRepository.UpdatePerson(userGuid, person);
                }
            else
            {
                Console.WriteLine("Error with: " + requestId);
            }
        }
    }
}