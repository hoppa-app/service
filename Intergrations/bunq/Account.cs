using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;

using hoppa.Service.Core;
using hoppa.Service.Data;
using hoppa.Service.Model;


namespace hoppa.Service.Intergrations.bunq
{
    public class bunqAccount : Account 
    {
        [Required]
        public int AccountId { get; set; }
        [Required]
        public int OwnerId { get; set; }
    }
    
    public class bunq
    {
        public static List<Account> GetAccounts(Person person)
        {
            string FunctionName = typeof(bunq).FullName + ".GetAccounts";

            Regex ibanRegex = new Regex("^([A-Za-z]{2}[0-9]{2})(?=(?:[ ]?[A-Za-z0-9]){10,30}$)((?:[ ]?[A-Za-z0-9]{3,5}){2,6})([ ]?[A-Za-z0-9]{1,3})?$");

            List<Account> accounts = new List<Account>();
            List<Connection> connections = person.Connections.FindAll(c => c.Type == "bunq");

            if(connections != null)
            {
                foreach(var connection in connections)
                {
                    var apiContext = ApiContext.FromJson(connection.Parameters["bunqContext"].ToString());
                    try
                    {
                        BunqContext.LoadApiContext(apiContext);
                    
                        var allPersonalMonetaryAccounts = MonetaryAccountBank.List().Value;

                        foreach (var monetaryAccount in allPersonalMonetaryAccounts)
                        {
                            if (monetaryAccount.Status == "ACTIVE")
                            {
                                foreach (var alias in monetaryAccount.Alias)
                                {
                                    if (ibanRegex.IsMatch(alias.Value))
                                    {
                                        var account = new bunqAccount
                                        {
                                            Guid = Support.CreateGuid(alias.Value),
                                            Type = "bunq",
                                            IBAN = alias.Value,
                                            AccountId = (int)monetaryAccount.Id,
                                            OwnerId = (int)monetaryAccount.UserId,
                                            Balance = (double)((monetaryAccount.Balance != null) ? Double.Parse(monetaryAccount.Balance.Value) : 0),
                                            Description = monetaryAccount.Description,
                                            AccessRights = (monetaryAccount.Balance != null) ? "read/write" : "read-only",
                                        };
                                        accounts.Add(account);
                                    }
                                }
                            }
                        }
                        
                        var allJointMonetaryAccounts = MonetaryAccountJoint.List().Value;

                        foreach (var monetaryAccount in allJointMonetaryAccounts)
                        {
                            if (monetaryAccount.Status == "ACTIVE")
                            {
                                foreach (var alias in monetaryAccount.Alias)
                                {
                                    if (ibanRegex.IsMatch(alias.Value))
                                    {
                                        var account = new bunqAccount
                                        {
                                            Guid = Support.CreateGuid(alias.Value),
                                            Type = "bunq",
                                            IBAN = alias.Value,
                                            AccountId = (int)monetaryAccount.Id,
                                            OwnerId = (int)monetaryAccount.UserId,
                                            Balance = (double)((monetaryAccount.Balance != null) ? Double.Parse(monetaryAccount.Balance.Value) : 0),
                                            Description = monetaryAccount.Description,
                                            AccessRights = (monetaryAccount.Balance != null) ? "read/write" : "read-only",
                                        };
                                        accounts.Add(account);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        Support.WriteToConsole("warn", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Is expired and will be deleted!");
                        
                        person.Connections.Remove(connection);
                        PersonRepository _personRepository = new PersonRepository();
                        var update = _personRepository.UpdatePerson(person.Guid, person).Result;
                    }
                }
            }
            return accounts;
        }
    }
}
