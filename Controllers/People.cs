using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using hoppa.Service.Core;
using hoppa.Service.Interfaces;
using hoppa.Service.Intergrations.bunq;
using hoppa.Service.Intergrations.Rabobank;
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
        public IActionResult Get()
        {
            var people = _personRepository.GetAllPeople().Result;

            if(people != null)
            {
                foreach(Person person in people)
                {
                    if(person.Accounts == null)
                    {
                        person.Accounts = new List<Account>();
                    }

                    if(person.Connections != null)
                    {
                        // Handle bunq accounts
                        if(person.Connections.FirstOrDefault(c => c.Type == "bunq") != null)
                        {
                            person.Accounts.AddRange(bunq.GetAccounts(person));
                        }
                        // Handle Rabobank accounts
                        if(person.Connections.FirstOrDefault(c => c.Type == "rabobank") != null)
                        {
                            person.Accounts.AddRange(Rabobank.GetAccounts(person));
                        }
                        // Remove sensitive data from response
                        foreach(Connection connection in person.Connections)
                        {
                            connection.Parameters = null;
                        }
                    }
                }

                return Ok(people);
            }
            else
            {
                return NotFound();
            }
        }

        [EnableQuery]
        public IActionResult Get(string key)
        {
            Person person = _personRepository.GetPerson(key).Result;

            if(person != null)
            {
                if(person.Accounts == null)
                {
                    person.Accounts = new List<Account>();
                }

                if(person.Connections != null)
                    {
                    // Handle bunq accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "bunq") != null)
                    {
                        person.Accounts.AddRange(bunq.GetAccounts(person));
                    }
                    // Handle Rabobank accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "rabobank") != null)
                    {
                        person.Accounts.AddRange(Rabobank.GetAccounts(person));
                    }
                    // Remove sensitive data from response
                    foreach(Connection connection in person.Connections)
                    {
                        connection.Parameters = null;
                    }
                }
                return Ok(person);
            }
            else
            {
                return NotFound();
            }
        }

        [EnableQuery]
        public IActionResult Post([FromBody] Person person)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            if(_personRepository.GetPerson(userGuid).Result == null)
            {
                person.Guid = userGuid;

                _personRepository.AddPerson(person);

                return Created(person);
            }
            else
            {
                return Conflict();
            }
            
 
        }

        [EnableQuery]
        public IActionResult Patch(string key, [FromBody] Delta<Person> delta)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            
            Person person = _personRepository.GetPerson(key).Result;

            if(person != null)
            {
                delta.Patch(person);
            
                _personRepository.UpdatePerson(key, person);

                return Updated(person);
            }
            else
            {
                return NotFound();
            }
        }

        [EnableQuery]
        public IActionResult Delete(string key)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            if(key == userGuid)
            {
                _personRepository.RemovePerson(key);

                return NoContent();
            }
            else
            {
                return Conflict();
            }
        }
    }

    [Authorize]
    public class PersonController : ODataController
    {
        private readonly IPersonRepository _personRepository;

        public PersonController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            Person person = _personRepository.GetPerson(userGuid).Result;
            
            if(person != null)
            {
                if(person.Connections != null)
                {
                    if(person.Accounts == null)
                    {
                        person.Accounts = new List<Account>();
                    }
                    // Handle bunq accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "bunq") != null)
                    {
                        person.Accounts.AddRange(bunq.GetAccounts(person));
                    }
                    // Handle Rabobank accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "rabobank") != null)
                    {
                        person.Accounts.AddRange(Rabobank.GetAccounts(person));
                    }
                    // Remove sensitive data from response
                    foreach(Connection connection in person.Connections)
                    {
                        connection.Parameters = null;
                    }
                }

                return Ok(person);
            }
            else
            {
                return NotFound();
            }
        }

        [EnableQuery]
        public IActionResult Patch([FromBody] Delta<Person> delta)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            Person person = _personRepository.GetPerson(userGuid).Result;

            if(person != null)
            {
                delta.Patch(person);

                _personRepository.UpdatePerson(userGuid, person);

                return Updated(person);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
