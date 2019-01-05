using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json.Linq;

using hoppa.Service.Core;
using hoppa.Service.Model;

namespace hoppa.Service.Intergrations.Rabobank
{
    public class RabobankAccount : Account 
    {
        [Required]
        public string AccountId { get; set; }
    }

    public class Rabobank
    {
        public static List<Account> GetAccounts(Person person)
        {
            List<Account> accounts = new List<Account>();
            List<Connection> connections = person.Connections.FindAll(c => c.Type == "rabobank");

            foreach(var connection in connections)
            {
                JObject response = Client.Get("/payments/account-information/ais/v3/accounts", (string)connection.Parameters["AccessToken"]);
                foreach(JObject account in response["accounts"])
                {
                    if((string)account["status"] == "enabled")
                    {
                        JObject balance = Client.Get("/payments/account-information/ais/v3/accounts/" + (string)account["resourceId"]+ "/balances", (string)connection.Parameters["AccessToken"]);
                        accounts.Add(new RabobankAccount
                            {
                                Guid = Support.CreateGuid((string)account["iban"]),
                                Type = "rabobank",
                                IBAN = (string)account["iban"],
                                AccountId = (string)account["resourceId"],
                                Balance = (double)balance["balances"][0]["balanceAmount"]["amount"],
                                AccessRights = "read-only",
                            }
                        );
                    }
                }
            }
            return accounts;
        }
    }
}