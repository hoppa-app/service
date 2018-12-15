/*https://oauth.sandbox.bunq.com/auth?response_type=code
&client_id=45faa6be4ee59555bc65b76ebec36e62a71b00487d667840ecd429e0256f531e
&redirect_uri=https://dev.hoppa.app/connect/bunq
&state=
*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;

using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;

using hoppa.Service.Core;
using hoppa.Service.Model;
using hoppa.Service.Interfaces;

namespace hoppa.Service.Intergrations.bunq
{
    public class RegistrationController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IPersonRepository _personRepository;
        private readonly IOptions<Configuration> _settings;

        public RegistrationController(IOptions<Configuration> settings, IPersonRepository personRepository, IHostingEnvironment hostingEnvironment)
        {
            _personRepository = personRepository;
            _settings = settings;
            _hostingEnvironment = hostingEnvironment;
        }
        [Authorize]
        [HttpGet("api/v1.0/connections/register/bunq")]
        public IActionResult bunq(string code = null)
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
                        code,
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
                string apiConfig = (new Register(_hostingEnvironment, accessToken)).bunqContext;
                
                var apiContext = ApiContext.FromJson(apiConfig);
                BunqContext.LoadApiContext(apiContext);

                Connection connection = new Connection(){
                    Guid = Guid.NewGuid().ToString(),
                    Type = "bunq",
                    AccessToken = accessToken,
                    ExternalId = BunqContext.UserContext.UserId,
                    Parameters = new Dictionary<string, object>
                    {
                        {"bunqContext", apiConfig }
                    }
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