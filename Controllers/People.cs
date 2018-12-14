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
    [Authorize]
    public class PeopleController : ODataController
    {
        private readonly IPersonRepository _personRepository;

        public PeopleController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        [EnableQuery]
        public async Task<IEnumerable<Person>>  Get()
        {
            var people = await _personRepository.GetAllPeople();
            foreach(Person person in people)
            {
                try
                {
                    person.Accounts.AddRange(BunqAccount.GetAccounts(person));
                }
                catch
                {

                }
            }

            return people;
        }

        [EnableQuery]
        public async Task<Person>  Get(string key)
        {
            var person = await _personRepository.GetPerson(key);
            
            try
            {
                person.Accounts.AddRange(BunqAccount.GetAccounts(person));
            }
            catch
            {

            }

            return person ?? new Person();
        }

        [EnableQuery]
        public IActionResult Post([FromBody] Person person)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            person.Guid = userGuid;

            _personRepository.AddPerson(person);

            return Created(person);
        }

        [EnableQuery]
        public IActionResult Delete(string key)
        {
            _personRepository.RemovePerson(key);

            return Ok();
        }
    }

    public class PersonController : ODataController
    {
        private readonly IPersonRepository _personRepository;

        public PersonController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        [EnableQuery]
        public async Task<Person> Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";

            var person = await _personRepository.GetPerson(userGuid);

            try
            {
                person.Accounts.AddRange(BunqAccount.GetAccounts(person));
            }
            catch
            {

            }
    
            return person ?? new Person();
        }

        [EnableQuery]
        public IActionResult Patch([FromBody] Person person)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";

            _personRepository.UpdatePerson(userGuid, person.DisplayName);

            return Ok();
        }
    }
}