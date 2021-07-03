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
using Hpl.HrmDatabase;
using Hpl.HrmDatabase.Services;
using Hpl.HrmDatabase.ViewModels;
using Newtonsoft.Json;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.Web.MdaemonServices;
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
            Log.Information("----START HAI PHAT LAND ACM----");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                    (utcDateTime, wt) => wt.File($"logs/acm-log-{utcDateTime}.txt"))
                .CreateLogger();

            //Console.WriteLine("Hello World!");

            //var res = await TestApiMdaemon();
            var res = await GetAllNhanVienErrorUser();
            Log.Information(JsonConvert.SerializeObject(res));

            //Console.WriteLine(res);
        }

        public async Task CreateUserAllSys(List<NhanVienViewModel> listNvs)
        {
            Log.Information("----START HAI PHAT LAND ACM----");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                    (utcDateTime, wt) => wt.File($"logs/acm-log-{utcDateTime}.txt"))
                .CreateLogger();

            foreach (var model in listNvs)
            {
                try
                {
                    string username = UsernameGenerator.CreateUsernameFromName(model.Ho, model.Ten);
                    PhongBan phongBan = UserService.GetPhongBanCap1CuaNhanVien(model.MaNhanVien);
                    string tenPhongBan = "HAI PHAT LAND COMPANY";
                    if (phongBan != null)
                    {
                        tenPhongBan = phongBan.Ten;
                    }
                    string ten = UsernameGenerator.ConvertToUnSign(model.Ten);
                    string ho = UsernameGenerator.ConvertToUnSign(model.Ho);
                    string dienThoai = "";
                    string pw = "Hpl@123";
                    string hoVaTen = ho + " " + ten;
                    try
                    {
                        dienThoai = "+84" + int.Parse(model.DienThoai.Trim());
                        pw = model.DienThoai.Trim().Substring(0, 6);
                    }
                    catch (Exception e)
                    {
                        Log.Error( model.MaPhongBan + " Số điện thoại lỗi " + e.Message);
                    }

                    //Tạo user trên AD
                    UserInfoAd userAd = new UserInfoAd
                    {
                        userPrincipalName = "",
                        sAMAccountName = username,
                        name = "",
                        sn = ten,
                        givenName = ho,
                        displayName = hoVaTen,
                        mail = "",
                        telephoneNumber = dienThoai,
                        department = tenPhongBan,
                        title = model.TenChucDanh,
                        employeeID = model.MaNhanVien,
                        description = "Created by tool. Time: " + DateTime.Now.ToString("G")
                    };
                    //TẠO USER AD
                    var userInfo = _passwordChangeProvider.CreateUser(userAd, pw).UserInfo;

                    if (userInfo != null)
                    {
                        //TẠO USER HRM
                        var nhanVien = UserService.CreateUserHrm(model.MaNhanVien, userInfo.sAMAccountName);
                        Log.Information(userInfo.sAMAccountName + " created on HRM.");
                    }
                    else
                    {
                        Log.Error("Không tạo được user trên AD cho mã Nhân Viên: " + model.MaNhanVien + ". Errors: ");
                    }

                    //TẠO EMAIL
                    CreateUserInput input = new CreateUserInput();
                    input.Domain = "haiphatland.com.vn";
                    input.Username = userInfo.sAMAccountName;
                    input.FirstName = ten;
                    input.LastName = ho;
                    input.FullName = hoVaTen;
                    input.Password = "Hpl@123";
                    input.AdminNotes = "Tạo từ tool, time: " + DateTime.Now.ToString("G");
                    input.MailList = "";
                    input.Group = "";
                    var res = await MdaemonXmlApi.CreateUser(input);
                    Log.Information(userInfo.sAMAccountName + " created on MDaemon. RES: " + JsonConvert.SerializeObject(res));

                }
                catch (Exception e)
                {
                    Log.Error("Error create user for MaNhanVien: " + model.MaNhanVien + ". Errors: " + e.Message);
                }
            }
        }

        public static async Task<List<NhanVienViewModel>> GetAllNhanVienErrorUser()
        {
            var db = new HrmDbContext();
            List<NhanVienViewModel> listNvs = new List<NhanVienViewModel>();

            listNvs = UserService.GetAllNhanVienErrorUser();

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
