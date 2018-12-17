using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;

using System.Threading.Tasks;
using System.Collections.Generic;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;
using hoppa.Service.Core;

using hoppa.Service.Intergrations.bunq;

namespace hoppa.Service.Controllers
{
    [Authorize]
    public class ConnectionsController : ODataController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IPersonRepository _personRepository;
        private readonly IOptions<Configuration> _settings;

        public ConnectionsController(IPersonRepository personRepository, IHostingEnvironment hostingEnvironment, IOptions<Configuration> settings)
        {
            _personRepository = personRepository;
            _hostingEnvironment = hostingEnvironment;
            _settings = settings;
        }
        
        [EnableQuery]
        public async Task<IEnumerable<Connection>> Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = await _personRepository.GetPerson(userGuid);
            
            if(person.Connections != null)
            {
                foreach(Connection connection in person.Connections)
                {
                    // Remove sensitive data from response
                    connection.Parameters = null;
                }
                return person.Connections;
            }
            else
            {
                return new List<Connection>();
            }
        }

        [EnableQuery]
        public IActionResult Post([FromBody] Registration registration)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";

            var client = new HttpClient();
            string accessToken = null;

            Connection connection = new Connection();
            connection.Guid = Guid.NewGuid().ToString();
            
            var person = (_personRepository.GetPerson(userGuid)).Result;

            switch (registration.Type)
            {
                case "bunq":
                    connection.Type = registration.Type;

                    // Try to get AccessToken
                    try
                    {
                        //Authentication Headers
                        var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("X-Bunq-Client-Request-Id", Guid.NewGuid().ToString())
                        };
                        var httpContent = new FormUrlEncodedContent(pairs);
                        
                        string url = String.Format(
                            "https://api-oauth.sandbox.bunq.com/v1/token?grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}",
                            registration.Code,
                            _settings.Value.Service.Intergrations.bunq.RedirectUri,
                            _settings.Value.Service.Intergrations.bunq.ClientId,
                            _settings.Value.Service.Intergrations.bunq.ClientSecret
                        );
                        //Get Access Token of the authorized user
                        Console.WriteLine(url);
                        accessToken = (string)JObject.Parse((client.PostAsync(url, httpContent).Result).Content.ReadAsStringAsync().Result)["access_token"];
                    }
                    catch
                    {
                        return BadRequest();
                    }

                    // Register and set bunqContext
                    string apiConfig = (new Context(_hostingEnvironment, accessToken)).bunqContext;
                    
                    connection.Parameters = new Dictionary<string, object>
                    {
                        {"bunqContext", apiConfig }
                    };
                    
                    // Gather connection details
                    var apiContext = ApiContext.FromJson(apiConfig);
                    BunqContext.LoadApiContext(apiContext);

                    connection.ExternalId = BunqContext.UserContext.UserId;
                    break;
                case "splitwise":
                    connection.Type = registration.Type;

                    // Try to get AccessToken
                    try
                    {
                        //Authentication Headers
                        var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "authorization_code"),
                            new KeyValuePair<string, string>("client_id", _settings.Value.Service.Intergrations.Splitwise.ClientId),
                            new KeyValuePair<string, string>("client_secret", _settings.Value.Service.Intergrations.Splitwise.ClientSecret),
                            new KeyValuePair<string, string>("redirect_uri", _settings.Value.Service.Intergrations.Splitwise.RedirectUri),
                            new KeyValuePair<string, string>("code", registration.Code)
                        };
                        var httpContent = new FormUrlEncodedContent(pairs);

                        //Get Access Token of the authorized user
                        accessToken = (string)JObject.Parse((client.PostAsync("https://secure.splitwise.com/oauth/token", httpContent).Result).Content.ReadAsStringAsync().Result)["access_token"];
                    }
                    catch
                    {
                        return BadRequest();
                    }
                    // Set AccessToken
                    connection.Parameters = new Dictionary<string, object>
                    {
                        {"AccessToken", accessToken }
                    };

                    // Gather connection details

                    //Set splitwise AccessToken
                    client = new HttpClient()
                    {
                        BaseAddress = new Uri("https://secure.splitwise.com/api/v3.0/")
                    };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)connection.Parameters["AccessToken"]);

                    //Get user details of authorized user at Splitwise
                    var connectedUser = JObject.Parse((client.GetAsync("get_current_user").Result).Content.ReadAsStringAsync().Result)["user"];

                    connection.ExternalId = (int)connectedUser["id"];
                    connection.DisplayName = (string)connectedUser["first_name"] + " " + connectedUser["last_name"];
                    connection.UserName = (string)connectedUser["email"];
                    break;
                default:
                break;
            }

            if(person.Connections == null)
            {
                person.Connections = new List<Connection>();
            }

            person.Connections.Add(connection);
        
            _personRepository.UpdatePerson(userGuid, person);

            // Remove sensitive data from response
            connection.Parameters = null;

            return Created(connection);
        }

        [EnableQuery]
        public IActionResult Delete(string key)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = (_personRepository.GetPerson(userGuid)).Result;

            var connection = person.Connections.FirstOrDefault(a => a.Guid == key); 
            
            person.Connections.Remove(connection);

            _personRepository.UpdatePerson(userGuid, person);

            return Ok();
        }
    }
}