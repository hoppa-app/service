using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;

using System.Threading.Tasks;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;

namespace hoppa.Service.Model
{
    public class Account
    {

        [Key]
        public string Guid { get; set; } = new Guid().ToString();
        [Required]
        public string Type { get; set; }
        [Required]
        public string IBAN { get; set; } = string.Empty;
        [Required]
        public string AccessRights { get; set; }

        public string Description { get; set; }
    }

    public class OtherAccount : Account
    {
        [Required]
        public string OwnerName { get; set; } = string.Empty;

    }
    public class BunqAccount : Account 
    {
        [Required]
        public int AccountId { get; set; }
        [Required]
        public int OwnerId { get; set; }
        public double Balance { get; set; }

        public static List<Account> GetAccounts(Person person)
        {
            List<int> bunqIds = new List<int>();
            List<Account> bunqAccounts = new List<Account>();
            Regex ibanRegex = new Regex("^([A-Za-z]{2}[0-9]{2})(?=(?:[ ]?[A-Za-z0-9]){10,30}$)((?:[ ]?[A-Za-z0-9]{3,5}){2,6})([ ]?[A-Za-z0-9]{1,3})?$");
            List<Connection> connections = person.Connections.FindAll(a => a.Type == "bunq"); 

            try
            {
                foreach(Connection connection in connections)
                {
                    bunqIds.Add(connection.ExternalId);
                }

                var allMonetaryAccounts = MonetaryAccountBank.List().Value;

                foreach (var monetaryAccount in allMonetaryAccounts)
                {
                    if (monetaryAccount.Status == "ACTIVE")
                    {
                        if (bunqIds.Contains((int)monetaryAccount.UserId))
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
                return bunqAccounts;
            }
            catch{

                return new List<Account>();
            }
        }
    }
}