using System;
using System.Collections.Generic;
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

namespace hoppa.Service.Intergrations.ING
{
    public class Headers
    {
        private static X509Certificate2 cert = Support.GetCertificate(Configuration.Current.Service.Intergrations.ING.Certificates.Signing.Thumbprint);
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
            return "keyId=\"" + Configuration.Current.Service.Intergrations.ING.ClientId + "\",algorithm=\"rsa-sha256\",headers=\"(request-target) date digest x-ing-reqid\",signature=\""+ComputeSignature(structure, cert)+"\"";
        }

        static string ComputeDigest(string rawData)  
        {  
            using (var shaHash = SHA256.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = shaHash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return "SHA-256=" + Convert.ToBase64String(bytes);  
            }  
        }

        static string ComputeSignature(string rawData, X509Certificate2 cert)
        {
            using (var rsa = cert.GetRSAPrivateKey())
            {
                // ComputeSignature - returns byte array  
                byte[] bytes = rsa.SignData(Encoding.UTF8.GetBytes(rawData), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(bytes);   
            }
        }
    }
    public class Client
    {
        private static HttpClient HttpClient()
        {
            X509Certificate2 cert = Support.GetCertificate(Configuration.Current.Service.Intergrations.ING.Certificates.TLS.Thumbprint);

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
            string structure = "(request-target): get "+ path.Split("?")[0] +"\ndate: "+headers.Date+"\ndigest: "+headers.Digest+"\nx-ing-reqid: "+headers.RequestId;

            headers.Signature = Headers.SignHeaders(structure);

            // Set Headers (ING)
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            httpClient.DefaultRequestHeaders.Add("X-ING-ReqID", headers.RequestId);
            httpClient.DefaultRequestHeaders.Add("Signature", headers.Signature);
            httpClient.DefaultRequestHeaders.Add("Digest", headers.Digest);
            httpClient.DefaultRequestHeaders.Add("Date", headers.Date);
            
            var response = httpClient.GetAsync(Configuration.Current.Service.Intergrations.ING.ApiUri + path);
            if(debug)
            {   
                Console.WriteLine("\nGET ("+Configuration.Current.Service.Intergrations.ING.ApiUri + path+")");
                Console.WriteLine("\nHeaders: \n\n" + httpClient.DefaultRequestHeaders);
                Console.WriteLine("\nSigned Data: \n\n" + structure);
            }           
            return JObject.Parse((response.Result).Content.ReadAsStringAsync().Result);
        }

        public static JObject Post(string path, List<KeyValuePair<string, string>> parameters, string accessToken = null, bool debug = false)
        {
            HttpClient httpClient = HttpClient();
            FormUrlEncodedContent httpContent = new FormUrlEncodedContent(parameters);

            // Get basic headers
            Headers headers = new Headers(httpContent.ReadAsStringAsync().Result);
            
            // Define singing stucture and sing.
            string structure = "(request-target): post "+ path.Split("?")[0] +"\ndate: "+headers.Date+"\ndigest: "+headers.Digest+"\nx-ing-reqid: "+headers.RequestId;

            headers.Signature = Headers.SignHeaders(structure);

            // Set Headers (ING)
            if(accessToken != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Signature", headers.Signature);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Signature", headers.Signature);
            }

            httpClient.DefaultRequestHeaders.Add("X-ING-ReqID", headers.RequestId);
            httpClient.DefaultRequestHeaders.Add("Digest", headers.Digest);
            httpClient.DefaultRequestHeaders.Add("Date", headers.Date);
            
            var response = httpClient.PostAsync(Configuration.Current.Service.Intergrations.ING.ApiUri + path, httpContent);
            if(debug)
            {   
                Console.WriteLine(response.Result);
                Console.WriteLine("\nPOST ("+Configuration.Current.Service.Intergrations.ING.ApiUri + path+")");
                Console.WriteLine("\nHeaders: \n\n" + httpClient.DefaultRequestHeaders);
                Console.WriteLine("\nSigned Data: \n\n" + structure);
            }           
            return JObject.Parse((response.Result).Content.ReadAsStringAsync().Result);
        }

        public static JObject Post(string path, JObject body, string accessToken = null, bool debug = false)
        {
            HttpClient httpClient = HttpClient();

            HttpContent httpContent = new StringContent(body.ToString());
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Get basic headers
            Headers headers = new Headers(httpContent.ReadAsStringAsync().Result);
            
            // Define singing stucture and sing.
            string structure = "(request-target): post "+ path.Split("?")[0] +"\ndate: "+headers.Date+"\ndigest: "+headers.Digest+"\nx-ing-reqid: "+headers.RequestId;

            headers.Signature = Headers.SignHeaders(structure);

            // Set Headers (ING)
            if(accessToken != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Signature", headers.Signature);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Signature", headers.Signature);
            }

            httpClient.DefaultRequestHeaders.Add("X-ING-ReqID", headers.RequestId);
            httpClient.DefaultRequestHeaders.Add("Digest", headers.Digest);
            httpClient.DefaultRequestHeaders.Add("Date", headers.Date);
            
            var response = httpClient.PostAsync(Configuration.Current.Service.Intergrations.ING.ApiUri + path, httpContent);
            if(debug)
            {   
                Console.WriteLine(response.Result);
                Console.WriteLine("\nPOST ("+Configuration.Current.Service.Intergrations.ING.ApiUri + path+")");
                Console.WriteLine("\nHeaders: \n\n" + httpClient.DefaultRequestHeaders);
                Console.WriteLine("\nSigned Data: \n\n" + structure);
            }           
            return JObject.Parse((response.Result).Content.ReadAsStringAsync().Result);
        }
    }
}