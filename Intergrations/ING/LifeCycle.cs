using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;

using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;

using hoppa.Service.Data;
using hoppa.Service.Model;
using hoppa.Service.Core;

namespace hoppa.Service.Intergrations.ING
{
    public class LifeCycle
    {
        public static void ValidateConsent()
        {
            string FunctionName = typeof(LifeCycle).FullName + ".ValidateConsent";
            PersonRepository _personRepository = new PersonRepository();
            while(true)
            {
                var start = DateTime.UtcNow;

                var people = _personRepository.GetAllPeople().Result;

                foreach(Person person in people)
                {
                    Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => Validating...");
                    if(person.Connections != null)
                    {
                        Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => Has List<Connection>");
                        List<Connection> connections = person.Connections.FindAll(c => c.Type == "ing");
                        if(connections.Count > 0)
                        {
                            Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => List<Connection> => Contains Type \"ING\"");
                            foreach(Connection connection in connections)
                            {
                                var consentedOn = Support.ConvertTimestamp((double)connection.Parameters["ConsentedOn"]);
                                if (start >= consentedOn.AddDays(90))
                                {
                                    Support.WriteToConsole("warn", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Is expired and will be deleted!");
                                    person.Connections.Remove(connection);
                                }
                                else if (start >= consentedOn.AddDays(85))
                                {
                                    Support.WriteToConsole("warn", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Is about to expire!");
                                   
                                    var client = new SendGridClient(Configuration.Current.Service.SendGrid.ApiKey);
                                    var from = new EmailAddress("noreply@hoppa.app", "hoppa.app");
                                    var subject = "Your ING consent is about to expire!";
                                    var to = new EmailAddress(person.EmailAddress, person.DisplayName);

                                    List<KeyValuePair<string, string>> parameters = null;
                                    JObject response = null;

                                    // Get Server AccessToken
                                    parameters = new List<KeyValuePair<string, string>>
                                    {
                                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                                        new KeyValuePair<string, string>("scope", "create_order granting payment-requests payment-requests:view payment-requests:create payment-requests:close virtual-ledger-accounts:fund-reservation:create virtual-ledger-accounts:fund-reservation:delete virtual-ledger-accounts:balance:view"),
                                    };

                                    response = Client.Post("/oauth2/token", parameters);
                                    string ServerAccessToken = (string)response["access_token"];

                                    // Get Authorization URL
                                    response = Client.Get("/oauth2/authorization-server-url?scope=view_balance&country_code=nl", ServerAccessToken);

                                    var plainTextContent = String.Format(
                                        "Renew consent via this link: {0}/response_type=code&client_id={1}&scope=view_balance&redirect_uri={2}&state={3}", 
                                        (string)response["location"],
                                        Configuration.Current.Service.Intergrations.ING.ClientId,
                                        Configuration.Current.Service.Intergrations.ING.RedirectUri,
                                        connection.Guid
                                    );
                                    var htmlContent = String.Format(
                                        "Renew consent via this <a href=\"{0}/response_type=code&client_id={1}&scope=view_balance&redirect_uri={2}&state={3}\">link</a>.<br/>{0}/response_type=code&client_id={1}&scope=view_balance&redirect_uri={2}&state={3}", 
                                        (string)response["location"],
                                        Configuration.Current.Service.Intergrations.ING.ClientId,
                                        Configuration.Current.Service.Intergrations.ING.RedirectUri,
                                        connection.Guid
                                    );

                                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                                    client.SendEmailAsync(msg);
                                }
                                else
                                {
                                    Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Is not Expired.");
                                }
                            }
                            var update = _personRepository.UpdatePerson(person.Guid, person);
                        }
                        else
                        {
                            Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => List<Connection> => Doesn't contain Type \"Rabobank\"");
                        }
                    }
                    else
                    {
                        Support.WriteToConsole("warn", FunctionName, "<Person> (" + person.Guid + ") => Has no List<Connection>");
                    }
                }
                var tomorrow = (start.AddDays(1).Date).AddHours(-1);
                var timeleft = tomorrow - start;
                Thread.Sleep((int)timeleft.TotalMilliseconds);
            }
        }

        public static void ValidateAccessToken()
        {
            string FunctionName = typeof(LifeCycle).FullName + ".ValidateAccessToken";
            PersonRepository _personRepository = new PersonRepository();
            while (true)
            {
                var start = DateTime.UtcNow;

                var people = _personRepository.GetAllPeople().Result;
                
                foreach(Person person in people)
                {
                    Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => Validating...");
                    if(person.Connections != null)
                    {
                        Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => Has List<Connection>");
                        List<Connection> connections = person.Connections.FindAll(c => c.Type == "ing");
                        if(connections.Count > 0)
                        {
                            Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => List<Connection> => Contains Type \"ING\"");
                            foreach(Connection connection in connections)
                            {
                                var validUntil = Support.ConvertTimestamp((double)connection.Parameters["Expiration"]);        
                                if(start.AddMinutes(5) >= validUntil) // allways refresh 5 minutes ealier
                                {
                                    Support.WriteToConsole("warn", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Is Expired!");
                                    Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Start Refresh...");
                                    
                                    // TODO: No refresh token at the moment.
                                    string code = "694d6ca9-1310-4d83-8dbb-e819c1ee6b80";
                                    
                                    JObject tokens = Intergrations.ING.Connnection.GetTokens(code);
                        
                                    if((string)tokens["access_token"] != null)
                                    {
                                        Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Refreshed!");
                                        int concentedOn = (int)connection.Parameters["ConsentedOn"];
                                        connection.Parameters = new Dictionary<string, object>
                                        {
                                            {"AccessToken", (string)tokens["access_token"] },
                                            {"Expiration", Support.ConvertTimestamp(DateTime.UtcNow.AddHours(1))},
                                            {"RefreshToken", (string)tokens["refresh_token"] },
                                            {"ConsentedOn", concentedOn }
                                        };
                                    }
                                    else
                                    {
                                        Support.WriteToConsole("error", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Refresh failed!");
                                    }
                                }
                                else
                                {
                                    Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Is not Expired.");
                                }
                            }
                            var update = _personRepository.UpdatePerson(person.Guid, person);
                        }
                        else
                        {
                            Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => List<Connection> => Doesn't contain Type \"ING\"");
                        }
                    }
                    else
                    {
                        Support.WriteToConsole("warn", FunctionName, "<Person> (" + person.Guid + ") => Has no List<Connection>");
                    }
                }
                var end = DateTime.UtcNow;
                var timeleft = start.AddMinutes(1) - end;
                Thread.Sleep((int)timeleft.TotalMilliseconds);
            }
        }
    }
}
