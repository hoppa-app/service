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

namespace hoppa.Service.Intergrations.Rabobank
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
                        List<Connection> connections = person.Connections.FindAll(c => c.Type == "rabobank");
                        if(connections.Count > 0)
                        {
                            Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => List<Connection> => Contains Type \"Rabobank\"");
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
                                    var subject = "Your Rabobank consent is about to expire!";
                                    var to = new EmailAddress(person.EmailAddress, person.DisplayName);
                                    var plainTextContent = String.Format(
                                        "Renew consent via this link: {0}/oauth2/authorize?response_type=code&client_id={1}&scope=AIS-Transactions-v2%20AIS-Balance-v2%20PaymentRequest&redirect_uri={2}&state={3}", 
                                        Configuration.Current.Service.Intergrations.Rabobank.ApiUri,
                                        Configuration.Current.Service.Intergrations.Rabobank.ClientId,
                                        Configuration.Current.Service.Intergrations.Rabobank.RedirectUri,
                                        connection.Guid
                                    );
                                    var htmlContent = String.Format(
                                        "Renew consent via this <a href=\"{0}/oauth2/authorize?response_type=code&client_id={1}&scope=AIS-Transactions-v2%20AIS-Balance-v2%20PaymentRequest&redirect_uri={2}&state={3}\">link</a>.<br/>{0}/oauth2/authorize?response_type=code&client_id={1}&scope=AIS-Transactions-v2%20AIS-Balance-v2%20PaymentRequest&redirect_uri={2}&state={3}", 
                                        Configuration.Current.Service.Intergrations.Rabobank.ApiUri,
                                        Configuration.Current.Service.Intergrations.Rabobank.ClientId,
                                        Configuration.Current.Service.Intergrations.Rabobank.RedirectUri,
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
                        List<Connection> connections = person.Connections.FindAll(c => c.Type == "rabobank");
                        if(connections.Count > 0)
                        {
                            Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => List<Connection> => Contains Type \"Rabobank\"");
                            foreach(Connection connection in connections)
                            {
                                var validUntil = Support.ConvertTimestamp((double)connection.Parameters["Expiration"]);        
                                if(start.AddMinutes(5) >= validUntil) // allways refresh 5 minutes ealier
                                {
                                    Support.WriteToConsole("warn", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Is Expired!");
                                    Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Start Refresh...");
                                    var client = new HttpClient();

                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue
                                    (
                                        "Basic", Convert.ToBase64String(
                                            Encoding.UTF8.GetBytes(
                                                String.Format(
                                                    "{0}:{1}", 
                                                    Configuration.Current.Service.Intergrations.Rabobank.ClientId,
                                                    Configuration.Current.Service.Intergrations.Rabobank.ClientSecret
                                                )
                                            )
                                        )
                                    );

                                    var pairs = new List<KeyValuePair<string, string>>
                                    {
                                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                                        new KeyValuePair<string, string>("refresh_token", (string)connection.Parameters["RefreshToken"])
                                    };
                                    
                                    var httpContent = new FormUrlEncodedContent(pairs);

                                    //Get Access Token  and new Refresh for the authorized user
                                    JObject response = JObject.Parse((client.PostAsync(Configuration.Current.Service.Intergrations.Rabobank.ApiUri + "/oauth2/token", httpContent).Result).Content.ReadAsStringAsync().Result);
                                    if((string)response["access_token"] != null)
                                    {
                                        Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => <Connection> (" + connection.Guid + ") => Refreshed!");
                                        connection.Parameters = new Dictionary<string, object>
                                        {
                                            {"AccessToken", (string)response["access_token"] },
                                            {"Expiration", Support.ConvertTimestamp(DateTime.UtcNow.AddHours(1))},
                                            {"RefreshToken", (string)response["refresh_token"] },
                                            {"ConsentedOn", (double)response["consented_on"] }
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
                            Support.WriteToConsole("info", FunctionName, "<Person> (" + person.Guid + ") => List<Connection> => Doesn't contain Type \"Rabobank\"");
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