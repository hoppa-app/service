using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;

using System.Threading.Tasks;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;

namespace hoppa.Service.Intergrations.bunq
{
    public class BunqAccount : Account 
    {
        [Required]
        public int AccountId { get; set; }
        [Required]
        public int OwnerId { get; set; }
        public double Balance { get; set; }

        public static List<Account> GetAccounts(Person person)
        {
            Regex ibanRegex = new Regex("^([A-Za-z]{2}[0-9]{2})(?=(?:[ ]?[A-Za-z0-9]){10,30}$)((?:[ ]?[A-Za-z0-9]{3,5}){2,6})([ ]?[A-Za-z0-9]{1,3})?$");

            List<Account> bunqAccounts = new List<Account>();
            List<Connection> bunqConnections = person.Connections.FindAll(c => c.Type == "bunq");

            if(bunqConnections != null)
            {
                foreach(var bunqConnection in bunqConnections)
                {
                    var apiContext = ApiContext.FromJson(bunqConnection.Parameters["bunqContext"].ToString());
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
                                    var account = new BunqAccount
                                    {
                                        Guid = new Guid(((int)monetaryAccount.Id), 0, 0, new byte[8]).ToString(),
                                        Type = "bunq",
                                        IBAN = alias.Value,
                                        AccountId = (int)monetaryAccount.Id,
                                        OwnerId = (int)monetaryAccount.UserId,
                                        Balance = (double)((monetaryAccount.Balance != null) ? Double.Parse(monetaryAccount.Balance.Value) : 0),
                                        Description = monetaryAccount.Description,
                                        AccessRights = (monetaryAccount.Balance != null) ? "read/write" : "read-only",
                                    };
                                    bunqAccounts.Add(account);
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
                                    var account = new BunqAccount
                                    {
                                        Guid = new Guid(((int)monetaryAccount.Id), 0, 0, new byte[8]).ToString(),
                                        Type = "bunq",
                                        IBAN = alias.Value,
                                        AccountId = (int)monetaryAccount.Id,
                                        OwnerId = (int)monetaryAccount.UserId,
                                        Balance = (double)((monetaryAccount.Balance != null) ? Double.Parse(monetaryAccount.Balance.Value) : 0),
                                        Description = monetaryAccount.Description,
                                        AccessRights = (monetaryAccount.Balance != null) ? "read/write" : "read-only",
                                    };
                                    bunqAccounts.Add(account);
                                }
                            }
                        }
                    }
                }
            }
            return bunqAccounts;
        }
    }
}
