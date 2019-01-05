using System;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace hoppa.Service.Core
{
    public class Support
    {
        public static string CreateGuid(string value)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(data).ToString();
        }
        public static double ConvertTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static DateTime ConvertTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static void WriteToConsole(string type, string classname, string value)
        {
            switch(type)
            {
                case "info":
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case "warn":
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    break;
                case "error":
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Red;                    
                    break;
            }
            Console.Write(type);
            Console.ResetColor();
            Console.WriteLine(": " + classname + "\n      " + value);
        }

        public static X509Certificate2 GetCertificate(string thumbprint){
            
            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                thumbprint,
                false);
            certStore.Close();
            
            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                return certCollection[0];
            }
            else
            {
                WriteToConsole("error", "hoppa.Service.Core.Support", "No Certificate found with Thumbprint (" + thumbprint + ")");
                return null;
            }
        }
    }
}