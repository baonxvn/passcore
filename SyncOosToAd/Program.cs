using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Hpl.Common;
using Hpl.HrmDatabase;
using Hpl.HrmDatabase.Services;
using Hpl.HrmDatabase.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;

namespace SyncOosToAd
{
    class Program
    {
        public static IConfigurationRoot configuration;
        private const string AppSettingsSectionName = "AppSettings";

        private static IOptions<PasswordChangeOptions> _options;
        private static ILogger _logger;
        private static IPasswordChangeProvider _passwordChangeProvider;

        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            var services = host.Services;
            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            _logger = provider.GetRequiredService<ILogger>();
            _passwordChangeProvider = provider.GetRequiredService<IPasswordChangeProvider>();
            _options = provider.GetRequiredService<IOptions<PasswordChangeOptions>>();

            _logger.Information("----START HAI PHAT LAND ACM----");
            Console.WriteLine("----START HAI PHAT LAND ACM----");
            int backDate = -1;
            try
            {
                //backDate = int.Parse(configuration.GetSection("AppSettings:BackDateSchedule").Value);
                backDate = int.Parse(_options.Value.BackDateSchedule);
            }
            catch (Exception e)
            {
                _logger.Error("Error get value BackDateSchedule: " + e);
            }

            //var listNvs = GetAllNhanVienErrorUser(backDate);
            //_logger.Information("----TỔNG SỐ HỒ SƠ XỬ LÝ: " + listNvs.Count);
            //Console.WriteLine("----TONG HO SO XU LY: " + listNvs.Count);
            //HplServices hplServices = new HplServices(_passwordChangeProvider, _logger);

            //await hplServices.CreateUserAllSys(listNvs);
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services
                        .Configure<PasswordChangeOptions>(configuration.GetSection(AppSettingsSectionName))
                        .AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>()
                        .AddSingleton((ILogger)new LoggerConfiguration()
                            .MinimumLevel.Verbose()
                            .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                            (utcDateTime, wt) => wt.File($"logs/acm-log-{utcDateTime}.txt"))
                        .CreateLogger()));

            //.WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //(utcDateTime, wt) =>
            //    wt.File($"logs/acm-log-{utcDateTime}.txt",
            //    outputTemplate: "[{Timestamp:yyyyMMdd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
        }

        public static List<NhanVienViewModel> GetAllNhanVienErrorUser(int backDate)
        {
            var dt = DateTime.Now.AddDays(backDate);

            var listNvs = UserService.GetAllNhanVienErrorUser(dt);

            return listNvs;
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
            //_logger.Information("GetUserInfo: " + username);
            var obj = _passwordChangeProvider.GetUserInfo(username, pw);

            return JsonConvert.SerializeObject(obj);
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
