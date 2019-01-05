using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Net.Http.Headers;

using hoppa.Service.Core;
using hoppa.Service.Interfaces;
using hoppa.Service.Model;

using Newtonsoft.Json.Linq;

namespace hoppa.Service.Intergrations.Rabobank
{
    public class Headers
    {
        private static string CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        private static X509Certificate2 cert = Support.GetCertificate(Configuration.Current.Service.Intergrations.Rabobank.Certificates.Signing.Thumbprint);
        public string Date { get; }
        public string Digest { get; }
        public string RequestId { get; }
        public string Certificate { get; }

        public string Signature { get; set; }

        public Headers(string body)
        {
            Date = DateTime.UtcNow.ToString("R");
            Digest = ComputeDigest(body);
            RequestId = Guid.NewGuid().ToString();
            Certificate = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
        }
        public static string SignHeaders(string structure)
        {
            return "keyId=\"" + Configuration.Current.Service.Intergrations.Rabobank.Certificates.Signing.SerialNr + "\",algorithm=\"rsa-sha512\",headers=\"date digest x-request-id\", signature=\""+ComputeSignature(structure, cert)+"\"";
        }

        static string ComputeDigest(string rawData)  
        {  
            using (var shaHash = SHA512.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = shaHash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return "SHA-512=" + Convert.ToBase64String(bytes);  
            }  
        }

        static string ComputeSignature(string rawData, X509Certificate2 cert)
        {
            using (var rsa = cert.GetRSAPrivateKey())
            {
                // ComputeSignature - returns byte array  
                byte[] bytes = rsa.SignData(Encoding.UTF8.GetBytes(rawData), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(bytes);   
            }
        }
    }
    public class Client
    {
        private static string CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        private static HttpClient HttpClient()
        {
            X509Certificate2 cert = Support.GetCertificate(Configuration.Current.Service.Intergrations.Rabobank.Certificates.TLS.Thumbprint);

            HttpClientHandler httpClientHandler = new HttpClientHandler() {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };        
            httpClientHandler.ClientCertificates.Add(cert);

            return new HttpClient(httpClientHandler);
        }

        public static JObject Get(string path, string accessToken = null, bool debug = false)
        {
            HttpClient httpClient = HttpClient();

            // Get basic headers
            Headers headers = new Headers(String.Empty);
            
            // Define singing stucture and sing.
            string structure = "date: "+headers.Date+"\ndigest: "+headers.Digest+"\nx-request-id: "+headers.RequestId;

            headers.Signature = Headers.SignHeaders(structure);

            // Set Headers (Rabobank)
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            httpClient.DefaultRequestHeaders.Add("X-IBM-Client-Id",Configuration.Current.Service.Intergrations.Rabobank.ClientId);
            httpClient.DefaultRequestHeaders.Add("X-Request-ID", headers.RequestId);
            httpClient.DefaultRequestHeaders.Add("Signature", headers.Signature);
            httpClient.DefaultRequestHeaders.Add("TPP-Signature-Certificate", headers.Certificate);
            httpClient.DefaultRequestHeaders.Add("Digest", headers.Digest);
            httpClient.DefaultRequestHeaders.Add("Date", headers.Date);
            
            var response = httpClient.GetAsync(Configuration.Current.Service.Intergrations.Rabobank.ApiUri + path);
            if(debug)
            {
                Console.WriteLine("\nGET ("+ Configuration.Current.Service.Intergrations.Rabobank.ApiUri + path+")");
                Console.WriteLine("\nHeaders: \n\n" + httpClient.DefaultRequestHeaders);
                Console.WriteLine("\nSigned Data: \n\n" + structure);
            }           
            return JObject.Parse((response.Result).Content.ReadAsStringAsync().Result);
        }
    }
}