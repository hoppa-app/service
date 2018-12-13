using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;

namespace hoppa.Service.Controllers
{
    [Authorize]
    [Route("api/v1.0/[controller]")]
    public class TestController : Controller
    {
        private readonly IPersonRepository _personRepository;

        public TestController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        // Call an initialization - api/system/init
        [HttpGet("{setting}")]
        public string Get(string setting)
        {
            if (setting == "init")
            {
                _personRepository.RemoveAllPeople();
                var name = _personRepository.CreateIndex();

                _personRepository.AddPerson(new Person()
                {
                    Guid = "6b9e605f-a484-4ecd-8e4b-9df459ef9ba9",
                    DisplayName = "Nick Duijvelshoff",
                    UpdatedOn = DateTime.Now,
                    Connections = new List<Connection>()
                    {
                        new Connection(){
                            Guid = "b3fa0f31-2bb3-4ef7-8d49-3d01df6f8291",
                            Type = "splitwise",
                            DisplayName = "Nick Duijvelshoff",
                            ExternalId = 14981522,
                            UserName = "nick@duijvelshoff.com",
                            AccessToken = "tej9yagoo8uth8iecohc9dohD9Eiphaivevie7Bo"
                        },
                        new Connection(){
                            Guid = "3f38f753-4371-4a7a-b788-55251c446da8",
                            Type = "bunq",
                            ExternalId = 6459
                        }
                    },
                    Accounts = new List<Account>()
                    {
                        new OtherAccount(){
                            Guid = "219864a4-2e6f-49f5-8fde-81700759387e",
                            Type = "other",
                            IBAN = "NL46INGB0666191506",
                            Description = "Personal ING Account",
                            OwnerName = "Hr N Duijvelshoff",
                            AccessRights = "limited"
                        }
                    },
                    Rules = new List<Rule>(){
                        new Rule(){
                            Guid = "4532e13c-f6e1-4ecf-b9b5-709775ebdd82",
                            Name = "Voorbeeld regel.",
                            Condition = new Tigger(){
                                Type = "trigger",
                                When = "1 0 0 0 0"
                            },
                            Actions = new List<Model.Action>(){
                                new Mail(){
                                    Type = "mail",
                                    Recipient = "online@hoppa.app"
                                },
                                new Payment(){
                                    Type = "payment",
                                    Account = new Account(){
                                        Type = "bunq",
                                        IBAN = "NL87BUNQ9900104358",
                                        AccessRights = "read/write"
                                    },
                                    Description = "Here do you have some money!",
                                    Recipient = new Pointer("EMAIL", "nick@hoppa.app", "Nick Duijvelshoff"),
                                    Amount = new Amount("10.00", "EUR")
                                }
                            }
                        }
                    }
                });

                _personRepository.AddPerson(new Person()
                {
                    Guid = "66443b70-59dd-483b-849e-d5683054ac6a",
                    DisplayName = "Patrick Witting",
                    UpdatedOn = DateTime.Now
                });

                _personRepository.AddPerson(new Person()
                {
                    Guid = "d2221f0e-ebc6-41c2-b13c-986ae1d26138",
                    DisplayName = "Jolanda Brinkhof",
                    UpdatedOn = DateTime.Now,
                });

                return "Collection \"hoppa\" was created and collection \"people\" was filled with 3 sample items";
            }

            return "Unknown";
        }
    }
}
