using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using hoppa.Service.Interfaces;
using hoppa.Service.Intergrations.Splitwise;

namespace hoppa.Service.Controllers
{
    [Authorize]
    public class SplitwiseController : ODataController
    {
        private readonly IPersonRepository _personRepository;

        public SplitwiseController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }
        
        [EnableQuery]
        public async Task<IEnumerable<Group>> Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://secure.splitwise.com/api/v3.0/")
            };
            var person = await _personRepository.GetPerson(userGuid);
            
            var groupList = new List<Group>();

            if(person.Connections != null)
            {
                var connections = person.Connections.FindAll(c => c.Type == "splitwise");

                foreach(var connection in connections)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer", 
                        (string)connection.Parameters["AccessToken"]
                    );

                    //Get personal user id
                    string splitwiseUserId = (string)(JObject.Parse((client.GetAsync("get_current_user").Result).Content.ReadAsStringAsync().Result))["user"]["id"];

                    JArray splitwiseGroups = (JArray)(JObject.Parse((client.GetAsync("get_groups").Result).Content.ReadAsStringAsync().Result))["groups"];

                    foreach (var splitwiseGroup in splitwiseGroups)
                    {
                        if((int)splitwiseGroup["id"] > 0)
                        {
                            double calculatedDept = 0;
                            if((bool)splitwiseGroup["simplify_by_default"])
                            {
                                foreach(var deptDetail in (JArray)splitwiseGroup["simplified_debts"])
                                {
                                    if((string)deptDetail["to"] == splitwiseUserId)
                                    {
                                        calculatedDept =+ Convert.ToDouble((string)deptDetail["amount"]);
                                    }
                                }
                            }
                            else
                            {
                                foreach(var deptDetail in (JArray)splitwiseGroup["original_debts"])
                                {
                                    if((string)deptDetail["to"] == splitwiseUserId)
                                    {
                                        calculatedDept =+ Convert.ToDouble((string)deptDetail["amount"]);
                                    }
                                }
                            }

                            groupList.Add( new Group(){
                                Guid = new Guid(((int)splitwiseGroup["id"]), 0, 0, new byte[8]).ToString(),
                                GroupId = (int)splitwiseGroup["id"],
                                Description = (string)splitwiseGroup["name"],
                                Balance = calculatedDept
                            });
                        }
                    }
                }
            }
            return groupList;
        }
    }
}