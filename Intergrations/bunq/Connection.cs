using Bunq.Sdk.Context;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace hoppa.Service.Intergrations.bunq
{
    public class Connection
    {
        public static void Register(IHostingEnvironment env, string apiKey)        
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
                apiContextSetup.Save();
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

                var apiContextSetup = ApiContext.Create(ApiEnvironmentType.SANDBOX, apiKey, "hoppa", ServicePlanIPs);
                apiContextSetup.Save();
            }
        }

        public static void Initialize()
        {
            var apiContext = ApiContext.Restore();

            BunqContext.LoadApiContext(apiContext);
        }
    }
}
