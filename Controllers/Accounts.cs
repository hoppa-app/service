using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using System.Collections.Generic;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;
using hoppa.Service.Intergrations.bunq;

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
        public async Task<IEnumerable<Account>> Get()
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = await _personRepository.GetPerson(userGuid);
            
            if(person.Connections != null)
            {
                // Handle bunq accounts
                if(person.Connections.FirstOrDefault(c => c.Type == "bunq") != null)
                {
                    person.Accounts.AddRange(BunqAccount.GetAccounts(person));
                }
            }
            
            return person.Accounts;
        }

        [EnableQuery]
        public async Task<Account> Get(string key)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = await _personRepository.GetPerson(userGuid);
            
            if(person.Connections != null)
            {
                // Handle bunq accounts
                if(person.Connections.FirstOrDefault(c => c.Type == "bunq") != null)
                {
                    person.Accounts.AddRange(BunqAccount.GetAccounts(person));
                }
            }

            var account = person.Accounts.FirstOrDefault(a => a.Guid == key);

            return account ?? new Account();
        }

        [EnableQuery]
        public IActionResult Post([FromBody] Account account)
        {
            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
                        
            var person = (_personRepository.GetPerson(userGuid)).Result;

            person.Accounts.Add(account);
        
            _personRepository.UpdatePerson(userGuid, person);

            return Created(account);
        }

        [EnableQuery]
        public IActionResult Delete(string key)
        {
            _personRepository.RemovePerson(key);

            string userGuid = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            //string userGuid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9";
            
            var person = (_personRepository.GetPerson(userGuid)).Result;

            var account = person.Accounts.FirstOrDefault(a => a.Guid == key); 
            
            person.Accounts.Remove(account);

            _personRepository.UpdatePerson(userGuid, person);

            return Ok();
        }
    }
}