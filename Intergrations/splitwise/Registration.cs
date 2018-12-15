using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

using hoppa.Service.Core;
using hoppa.Service.Model;
using hoppa.Service.Interfaces;

namespace hoppa.Service.Intergrations.splitwise
{
    public class RegistrationController : Controller
    {
        private readonly IPersonRepository _personRepository;
        private readonly IOptions<Configuration> _settings;

        public RegistrationController(IOptions<Configuration> settings, IPersonRepository personRepository)
        {
            _personRepository = personRepository;
            _settings = settings;
        }
        [Authorize]
        [HttpGet("api/v1.0/connections/register/splitwise")]
        public IActionResult Splitwise(string code = null)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";

            if (code == null)
            {
                return  BadRequest();
            }
            else
            {
                var client = new HttpClient();
                string accessToken = null;

                //Authentication Headers
                var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", _settings.Value.Service.Intergrations.Splitwise.ClientId),
                    new KeyValuePair<string, string>("client_secret", _settings.Value.Service.Intergrations.Splitwise.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", _settings.Value.Service.Intergrations.Splitwise.RedirectUri),
                    new KeyValuePair<string, string>("code", code)
                };
                var httpContent = new FormUrlEncodedContent(pairs);

                try
                {
                    //Get Access Token of the authorized user
                    accessToken = (string)JObject.Parse((client.PostAsync("https://secure.splitwise.com/oauth/token", httpContent).Result).Content.ReadAsStringAsync().Result)["access_token"];
                }
                catch
                {
                    return BadRequest();
                }

                //Set AccessToken for authorized user 
                client = new HttpClient()
                {
                    BaseAddress = new Uri("https://secure.splitwise.com/api/v3.0/")
                };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Get user details of authorized user at Splitwise
                var connectedUser = JObject.Parse((client.GetAsync("get_current_user").Result).Content.ReadAsStringAsync().Result)["user"];

                var connection = new Connection(){
                    Guid = Guid.NewGuid().ToString(),
                    Type = "splitwise",
                    AccessToken = accessToken,
                    DisplayName = (string)connectedUser["first_name"] + " " + connectedUser["last_name"],
                    UserName = (string)connectedUser["email"],
                    ExternalId = (int)connectedUser["id"]
                };

                var person = _personRepository.GetPerson(userGuid).Result;

                if(person.Connections == null)
                {
                    person.Connections = new List<Connection>();
                }

                person.Connections.Add(connection);
            
                _personRepository.UpdatePerson(userGuid, person);

                JObject response = new JObject(){
                    { "guid", connection.Guid}
                };

                return Ok(response);
            }
        }
    }
}