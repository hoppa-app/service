using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using System.Collections.Generic;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;

using hoppa.Service.Intergrations.bunq;

namespace hoppa.Service.Controllers
{
    [Authorize]
    public class ConnectionsController : ODataController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IPersonRepository _personRepository;

        public ConnectionsController(IPersonRepository personRepository, IHostingEnvironment hostingEnvironment)
        {
            _personRepository = personRepository;
            _hostingEnvironment = hostingEnvironment;
        }
        
        [EnableQuery]
        public async Task<IEnumerable<Connection>> Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = await _personRepository.GetPerson(userGuid);
            
            if(person.Connections != null)
            {
                foreach(Connection connection in person.Connections)
                {
                    // Remove sensitive data from response
                    connection.AccessToken = null;
                    connection.Parameters = null;
                }
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
            
            if(connection.Type == "bunq")
            {
                connection.Parameters = new Dictionary<string, object>
                {
                    {"bunqContext", (new Register(_hostingEnvironment, connection.AccessToken)).bunqContext}
                };
            }

            if(person.Connections == null)
            {
                person.Connections = new List<Connection>();
            }

            person.Connections.Add(connection);
        
            _personRepository.UpdatePerson(userGuid, person);

            // Remove sensitive data from response
            connection.AccessToken = null;
            connection.Parameters = null;
             
            return Created(connection);
        }

        [EnableQuery]
        public IActionResult Delete(string key)
        {
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