using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using System.Collections.Generic;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;

namespace hoppa.Service.Controllers
{
    //[Authorize]
    public class RulesController : ODataController
    {
        private readonly IPersonRepository _personRepository;

        public RulesController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }
        
        [EnableQuery]
        public async Task<IEnumerable<Rule>> Get()
        {
            //string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";

            var person = await _personRepository.GetPerson(userGuid);
            
            if(person.Rules != null)
            {
                return person.Rules;
            }
            else
            {
                return new List<Rule>();
            }
        }
    }
}