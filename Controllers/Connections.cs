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
    public class ConnectionsController : ODataController
    {
        private readonly IPersonRepository _personRepository;

        public ConnectionsController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }
        
        [EnableQuery]
        public async Task<IEnumerable<Connection>> Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = await _personRepository.GetPerson(userGuid);
            
            if(person.Connections != null)
            {
                return person.Connections;
            }
            else
            {
                return new List<Connection>();
            }
        }

        [EnableQuery]
        public IActionResult Post([FromBody] Connection connection)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";

            connection.Guid = Guid.NewGuid().ToString();
            
            var person = (_personRepository.GetPerson(userGuid)).Result;

            person.Connections.Add(connection);
        
            _personRepository.UpdatePerson(userGuid, person);

            return Created(connection);
        }

        [EnableQuery]
        public IActionResult Delete(string key)
        {
            _personRepository.RemovePerson(key);

            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = (_personRepository.GetPerson(userGuid)).Result;

            var connection = person.Connections.FirstOrDefault(a => a.Guid == key); 
            
            person.Connections.Remove(connection);

            _personRepository.UpdatePerson(userGuid, person);

            return Ok();
        }
    }
}