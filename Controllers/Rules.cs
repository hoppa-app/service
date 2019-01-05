using System.Linq;

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using hoppa.Service.Core;
using hoppa.Service.Interfaces;
using hoppa.Service.Model;

namespace hoppa.Service.Controllers
{
    [Authorize]
    public class RulesController : ODataController
    {
        private readonly IPersonRepository _personRepository;

        public RulesController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }
        
        [EnableQuery]
        public IActionResult Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            var person = _personRepository.GetPerson(userGuid).Result;
            
            if(person != null)
            {
                if(person.Rules != null)
                {
                    return Ok(person.Rules);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }
    }
}