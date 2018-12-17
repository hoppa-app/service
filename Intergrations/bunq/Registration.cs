using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using hoppa.Service.Core;
using hoppa.Service.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;

namespace hoppa.Service.Intergrations.bunq
{
    public class RegistrationController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IPersonRepository _personRepository;
        private readonly IOptions<Configuration> _settings;

        public RegistrationController(IPersonRepository personRepository, IHostingEnvironment hostingEnvironment, IOptions<Configuration> settings)
        {
            _personRepository = personRepository;
            _hostingEnvironment = hostingEnvironment;
            _settings = settings;
        }

        [Authorize]
        [HttpGet("api/v1.0/connections/bunq")]
        public IActionResult Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Bunq-Client-Request-Id", "unique");

            var result = JObject.Parse((client.PostAsync("https://api.tinker.bunq.com/v1/credential-password-ip-request", null).Result).Content.ReadAsStringAsync().Result);

            string requestId = (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["uuid"];
            string requestQR = (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["qr_base64"];
            
            string connectionGuid = Guid.NewGuid().ToString();
            
            Task.Run(() => Validate.Run(
                _personRepository,
                _hostingEnvironment,
                _settings,
                userGuid, 
                connectionGuid, 
                requestId
            ));

            JObject response = new JObject(){
                {"status", "created"},
                {"data", new JObject(){
                    {"connection", new JObject(){
                        {"guid", connectionGuid}
                    }},
                    {"request", new JObject(){
                        {"qr", requestQR}
                    }}
                }}
            };

            return Ok(response);
        }
    }
}