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
    public class AccountsController : ODataController
    {
        private readonly IPersonRepository _personRepository;
        
        public AccountsController(IPersonRepository personRepository)
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
                }
                if(person.Accounts.Count > 0)
                {
                    return Ok(person.Accounts);
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
            
            Person person = _personRepository.GetPerson(userGuid).Result;
            
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
                }
                
                Account account = person.Accounts.FirstOrDefault(a => a.Guid == key);

                if(account != null)
                {
                    return Ok(account);
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
        public IActionResult Post([FromBody] Account account)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
                        
            Person person = (_personRepository.GetPerson(userGuid)).Result;

            if(person != null && account.IBAN != null)
            {
                account.Guid = Support.CreateGuid(account.IBAN);

                if(person.Accounts == null)
                {
                    person.Accounts = new List<Account>();
                }

                // Handle connected accounts
                List<Account> connectedAccounts = new List<Account>();
                if(person.Connections != null)
                {
                    // Handle bunq accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "bunq") != null)
                    {
                        connectedAccounts.AddRange(bunq.GetAccounts(person));
                    }
                    // Handle Rabobank accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "rabobank") != null)
                    {
                        connectedAccounts.AddRange(Rabobank.GetAccounts(person));
                    }
                }

                if(
                    person.Accounts.FirstOrDefault(a => a.Guid == account.Guid) == null &&
                    connectedAccounts.FirstOrDefault(a => a.Guid == account.Guid) == null
                )
                {
                    person.Accounts.Add(account);
                    _personRepository.UpdatePerson(userGuid, person);

                    return Created(account);
                }
                else
                {
                    return Conflict();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [EnableQuery]
        public IActionResult Patch(string key, [FromBody] Delta<Account> delta)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
                        
            Person person = (_personRepository.GetPerson(userGuid)).Result;

            if(person != null)
            {
                // Handle connected accounts
                List<Account> connectedAccounts = new List<Account>();
                if(person.Connections != null)
                {
                    // Handle bunq accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "bunq") != null)
                    {
                        connectedAccounts.AddRange(bunq.GetAccounts(person));
                    }
                    // Handle Rabobank accounts
                    if(person.Connections.FirstOrDefault(c => c.Type == "rabobank") != null)
                    {
                        connectedAccounts.AddRange(Rabobank.GetAccounts(person));
                    }
                }

                Account account = person.Accounts.FirstOrDefault(a => a.Guid == key);

                if(account != null)
                {
                    delta.Patch(account);

                    if(
                        person.Accounts.FirstOrDefault(a => a.IBAN == account.IBAN) == account &&
                        connectedAccounts.FirstOrDefault(a => a.IBAN == account.IBAN) == null
                    )
                    {
                        _personRepository.UpdatePerson(userGuid, person);

                        return Updated(account);
                    }
                    else
                    {
                        return Conflict();
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
                Account account = person.Accounts.FirstOrDefault(a => a.Guid == key); 

                if(account != null)
                {
                    person.Accounts.Remove(account);

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
                return BadRequest();
            }
        }
    }
}
