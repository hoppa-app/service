using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;
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
        public IActionResult Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            Person person = _personRepository.GetPerson(userGuid).Result;

            if(person != null)
            {
                if(person.Connections != null)
                {
                    foreach(Connection connection in person.Connections)
                    {
                        // Remove sensitive data from response
                        connection.Parameters = null;
                    }
                    return Ok(person.Connections);
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

        [EnableQuery]
        public IActionResult Get(string key)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            
            var person = _personRepository.GetPerson(userGuid).Result;

            if(person != null)
            {
                if(person.Connections != null)
                {
                    foreach(Connection connectionDetails in person.Connections)
                    {
                        // Remove sensitive data from response
                        connectionDetails.Parameters = null;
                    }

                    Connection connection = person.Connections.FirstOrDefault(a => a.Guid == key);
                    
                    if(connection != null)
                    {
                        return Ok(connection);
                    }
                    else
                    {
                        return NotFound();
                    }
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

        [EnableQuery]
        public IActionResult Post([FromBody] Registration registration)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            Person person = (_personRepository.GetPerson(userGuid)).Result;

            if(person != null)
            {
                JObject tokens;

                Connection connection = null;
                        
                switch (registration.Type)
                {
                    case "bunq":

                        tokens = Intergrations.bunq.Connnection.GetTokens(registration.Code);
                        
                        if((string)tokens["access_token"] != null)
                        {
                            connection = new Intergrations.bunq.Connnection(_hostingEnvironment, tokens);
                        }
                        else
                        {
                            connection = new Intergrations.bunq.Connnection(_hostingEnvironment, registration.Code);
                        }
                        if(connection == null)
                        {
                            return BadRequest();
                        }
                        
                        break;
                    case "rabobank":
                        
                        tokens = Intergrations.Rabobank.Connnection.GetTokens(registration.Code);
                        
                        if((string)tokens["access_token"] != null)
                        {
                            connection = new Intergrations.Rabobank.Connnection(tokens);
                        }
                        else
                        {
                            return BadRequest();
                        }
                        
                        break;
                    case "ing":
                        
                        tokens = Intergrations.ING.Connnection.GetTokens(registration.Code);
                        
                        if((string)tokens["access_token"] != null)
                        {
                            connection = new Intergrations.ING.Connnection(tokens);
                        }
                        else
                        {
                            return BadRequest();
                        }
                        
                        break;
                    case "splitwise":
                        
                        tokens = Intergrations.Splitwise.Connnection.GetTokens(registration.Code);
                        
                        if((string)tokens["access_token"] != null)
                        {
                            connection = new Intergrations.Splitwise.Connnection(tokens);
                        }
                        else
                        {
                            return BadRequest();
                        }
                        
                        break;
                    default:
                        return BadRequest();
                }

                connection.Guid = System.Guid.NewGuid().ToString();

                if(person.Connections == null)
                {
                    person.Connections = new List<Connection>();
                }

                person.Connections.Add(connection);
            
                _personRepository.UpdatePerson(userGuid, person);

                // Remove sensitive data from response
                connection.Parameters = null;

                return Created(connection);
            }
            else
            {
                return BadRequest();
            }
        }
        public IActionResult Patch(string key, [FromBody] Registration registration)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            Person person = (_personRepository.GetPerson(userGuid)).Result;

            if(person != null)
            {
                if(person.Connections != null)
                {
                    JObject tokens;

                    Connection connection = person.Connections.FirstOrDefault(a => a.Guid == key);

                    if(connection != null)
                    {
                        switch (registration.Type)
                        {
                            case "rabobank":
                                tokens = Intergrations.Rabobank.Connnection.GetTokens(registration.Code);
                        
                                if((string)tokens["access_token"] != null)
                                {
                                    connection = new Intergrations.Rabobank.Connnection(tokens);
                                }
                                else
                                {
                                    return BadRequest();
                                }
                                break;
                            case "ing":
                                tokens = Intergrations.ING.Connnection.GetTokens(registration.Code);
                        
                                if((string)tokens["access_token"] != null)
                                {
                                    connection = new Intergrations.ING.Connnection(tokens);
                                }
                                else
                                {
                                    return BadRequest();
                                }
                                break;
                            default:
                                return BadRequest();
                        }

                        _personRepository.UpdatePerson(userGuid, person);

                        return Updated(connection);
                    }
                    else
                    {
                        return NotFound();
                    }
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

        [EnableQuery]
        public IActionResult Delete(string key)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            
            Person person = (_personRepository.GetPerson(userGuid)).Result;

            if(person != null)
            {
                if(person.Connections != null)
                {
                    Connection connection = person.Connections.FirstOrDefault(a => a.Guid == key); 

                    if(connection != null)
                    {
                        person.Connections.Remove(connection);

                        _personRepository.UpdatePerson(userGuid, person);

                        return NoContent();
                    }
                    else
                    {
                        return NotFound();
                    }
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