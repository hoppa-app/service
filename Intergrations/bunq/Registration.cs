using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;
using hoppa.Service.Interfaces;

namespace hoppa.Service.Intergrations.bunq
{
    public class RegistrationController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public RegistrationController(IPersonRepository personRepository, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        [HttpGet("api/v1.0/connections/bunq")]
        public IActionResult Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;            

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Bunq-Client-Request-Id", "unique");

            var result = JObject.Parse((client.PostAsync("https://api.tinker.bunq.com/v1/credential-password-ip-request", null).Result).Content.ReadAsStringAsync().Result);

            string requestId = (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["uuid"];
            string requestQR = (string)result["Response"][0]["UserCredentialPasswordIpRequest"]["qr_base64"];
            
            string connectionGuid = Guid.NewGuid().ToString();
            
            Task.Run(() => Connnection.ValidateStatus(
                _hostingEnvironment,
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