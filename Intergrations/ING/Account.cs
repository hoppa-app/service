using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;
using hoppa.Service.Model;

namespace hoppa.Service.Intergrations.ING
{
    public class INGAccount : Account 
    {
        [Required]
        public string AccountId { get; set; }

        [Required]
        public string OwnerName { get; set; }
    }

    public class ING
    {
        public static List<Account> GetAccounts(Person person)
        {
            List<Account> accounts = new List<Account>();
            List<Connection> connections = person.Connections.FindAll(c => c.Type == "ing");

            foreach(var connection in connections)
            {
                JObject response = Client.Get("/v1/accounts", (string)connection.Parameters["AccessToken"]);
                foreach(JObject account in response["accounts"])
                {
                    //JObject balance = Client.Get("/v1/accounts/" + (string)account["accountId"]+ "/balances", (string)connection.Parameters["AccessToken"], true);
                    accounts.Add(new INGAccount
                        {
                            Guid = Support.CreateGuid((string)account["iban"]),
                            Type = "ing",
                            IBAN = (string)account["iban"],
                            AccountId = (string)account["accountId"],
                            OwnerName = (string)account["name"],
                            //Balance = (double)balance["balances"][0]["balanceAmount"]["amount"],
                            AccessRights = "read-only",
                        }
                    );
                }
            }
            return accounts;
        }
    }
}