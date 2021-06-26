using System;
using System.Buffers.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.Web.Models;

namespace SyncOosToAd
{
    class Program
    {
        private static readonly ILogger _logger;
        private readonly ClientSettings _options;
        private readonly IPasswordChangeProvider _passwordChangeProvider;


        static async Task Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Information()
            //    .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //        (utcDateTime, wt) => wt.File($"logs/LDAP_Win-log-{utcDateTime}.txt"))
            //    .CreateLogger();

            //Console.WriteLine("Hello World!");
            //Log.Information("testLog");

            var res = await TestApiMdaemon();

            Console.WriteLine(res);
        }


        /// <summary>
        /// Test Active Directory
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        public string GetUserInfo(string username, string pw)
        {
            //username += "@haiphatland.local";
            username += "@baonx.com";
            _logger.Information("GetUserInfo: " + username);
            var obj = _passwordChangeProvider.GetUserInfo(username, pw);

            return JsonConvert.SerializeObject(obj);
        }

        public void TestConnectAd()
        {

        }

        public static async Task<string> TestApiMdaemon()
        {
            Uri url = new Uri("https://172.168.0.60:444/MdMgmtWS");
            var xmlFile = Directory.GetCurrentDirectory() + "/XmlApi/GetUserInfo.xml";
            var domain = "company.test";
            var user1 = "baonx@company.test";
            var user2 = "testapi@company.test";
            var pass = "Admin@123";
            //var abc= objHttp.setRequestHeader("Authorization", "Basic " + Base64.("charles.xavier@x-men.int:Password"));
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(user1 + ":" + pass));

            //Sử dụng HttpClient
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri("https://172.168.0.60:444/MdMgmtWS");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", svcCredentials);

            try
            {
                XmlDocument docRequest = new XmlDocument();
                docRequest.Load(xmlFile);
                Console.WriteLine(docRequest.InnerXml);
                var httpContent = new StringContent(docRequest.InnerXml, Encoding.UTF8, "text/xml");

                var respone = await client.PostAsync(url, httpContent);

                return respone.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Sử dụng WebRequest và HttpWebRequest
            //var request = (HttpWebRequest)WebRequest.Create(url);
            //request.Headers.Add("Authorization", "Basic " + svcCredentials
            //
            //The remote certificate is invalid according to the validation procedure: RemoteCertificateNameMismatch, RemoteCertificateChainErrors
        }
    }
}
