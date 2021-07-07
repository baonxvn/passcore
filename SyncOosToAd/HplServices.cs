using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using Hpl.HrmDatabase;
using Hpl.HrmDatabase.Services;
using Hpl.HrmDatabase.ViewModels;
using Hpl.SaleOnlineDatabase;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using SyncOosToAd.MyService;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;
using Unosquare.PassCore.Web.MdaemonServices;
using NhanVienSale = Hpl.SaleOnlineDatabase.NhanVien;
using PhongBan = Hpl.HrmDatabase.PhongBan;

namespace SyncOosToAd
{
    public class HplServices
    {
        private readonly ILogger _logger;
        private readonly IPasswordChangeProvider _passwordChangeProvider;

        public HplServices(IPasswordChangeProvider passwordChangeProvider, ILogger logger)
        {
            _passwordChangeProvider = passwordChangeProvider;
            _logger = logger;
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Information()
            //    .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //        (utcDateTime, wt) => wt.File($"logs/acm-log-{utcDateTime}.txt"))
            //    .CreateLogger();
            //Log.Information("----START HAI PHAT LAND ACM----");
        }

        public async Task CreateUserAllSys(List<NhanVienViewModel> listNvs)
        {
            string userListEmail = "<p style=\"font-weight: bold\">DANH SÁCH USER ĐÃ TẠO MỚI HOẶC SỬA THÔNG TIN</p>";
            string userSale = "<p style=\"font-weight: bold\">DANH SÁCH USER ĐÃ TẠO SALE ONLINE</p>";
            int i = 0;
            int j = 0;
            foreach (var model in listNvs)
            {
                //if (!model.MaNhanVien.Equals("KD8-332")) continue;
                try
                {
                    i++;
                    string userName = UsernameGenerator.CreateUsernameFromName(model.Ho, model.Ten);
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
                        Log.Error(model.MaPhongBan + " Số điện thoại lỗi " + e.Message);
                    }

                    //Tạo user trên AD
                    UserInfoAd userAd = new UserInfoAd
                    {
                        userPrincipalName = "",
                        sAMAccountName = userName,
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
                    var userInfoAd = _passwordChangeProvider.CreateAdUser(userAd, pw);
                    var userInfo = userInfoAd.UserInfo;

                    userName = userInfo.sAMAccountName;

                    //TẠO USER HRM
                    var nhanVien = UserService.CreateUserHrm2(model, userName);
                    _logger.Information(userName + " created on HRM at " + DateTime.Now.ToString("G"));

                    //TẠO EMAIL
                    CreateUserInput input = new CreateUserInput();
                    input.Domain = "haiphatland.com.vn";
                    input.Username = userName;
                    input.FirstName = ten;
                    input.LastName = ho;
                    input.FullName = hoVaTen;
                    input.Password = "Hpl@123";
                    input.AdminNotes = "Tạo từ tool, time: " + DateTime.Now.ToString("G");
                    input.MailList = "";
                    input.Group = "";
                    var res = await MdaemonXmlApi.CreateUser(input);

                    _logger.Information(userName + " created on MDaemon at " + DateTime.Now.ToString("G"));

                    //Update log vào DB
                    HplSyncLog syncLog = new HplSyncLog
                    {
                        UserName = userName,
                        MaNhanVien = model.MaNhanVien,
                        Payload = JsonConvert.SerializeObject(nhanVien),
                        LogForSys = "HRM"
                    };
                    AbpServices.AddSyncLogAbp(syncLog);

                    //TẠO USER TRÊN SALEONLINE
                    if (!SaleOnlineServices.IsNhanVienExist(userName))
                    {
                        var nvSale = new NhanVienSale();
                        nvSale.MaSo = userName;
                        //nvSale.MatKhau = model.
                        nvSale.HoTen = model.Ho + " " + model.Ten;
                        nvSale.Ho = model.Ho;
                        nvSale.Ten = model.Ten;
                        nvSale.DienThoai = model.DienThoai;
                        nvSale.Email = nhanVien.Email;
                        //nvSale.NgaySinh = model.
                        nvSale.UserType = 1;
                        nvSale.Lock = false;
                        nvSale.SoCmnd = model.CMTND;
                        nvSale.KeyCode = model.MaNhanVien;
                        nvSale.MaNvcn = 8;
                        nvSale.IsDeleted = false;

                        //Xác định BranchId trên SaleOnline
                        var phongBanCap1 = UserService.GetPhongBanCap1CuaNhanVien(model.MaNhanVien);
                        var branch = SaleOnlineServices.GetBranchId(phongBanCap1.MaPhongBan);
                        if (branch != null)
                        {
                            j++;
                            nvSale.BranchId = branch.BranchId;
                            SaleOnlineServices.CreateUserSale(nvSale);

                            syncLog = new HplSyncLog
                            {
                                UserName = userName,
                                MaNhanVien = model.MaNhanVien,
                                Payload = JsonConvert.SerializeObject(nvSale),
                                LogForSys = "SaleOnline"
                            };
                            AbpServices.AddSyncLogAbp(syncLog);

                            userSale += j + ". " + model.Ho + " " + model.Ten + " - " + model.MaNhanVien + " - " + userName + "<br />";

                            _logger.Information(userName + " created on SaleOnline at " + DateTime.Now.ToString("G"));
                        }
                    }

                    userListEmail += i + ". <a href=\"https://hrm.haiphatland.com.vn/HRIS/Profile/Index/" +
                                     model.NhanVienID + "\">" + model.Ho + " " + model.Ten + "</a>. Mã NV: " +
                                     model.MaNhanVien + ". User: " + userInfo.sAMAccountName + "<br />";
                    //userListEmail += "https://hrm.haiphatland.com.vn/HRIS/Profile/Index/15296 <br>";
                }
                catch (Exception e)
                {
                    _logger.Error("Error create user for MaNhanVien: " + model.MaNhanVien +
                                  "  . Errors: " + e);
                }
            }

            if (listNvs.Count > 0)
            {
                if (j > 0) userListEmail += userSale;

                userListEmail += "Lưu ý:<br />" +
                                 "1. Pass mặc định của mail là Hpl@123.<br />" +
                                 "2. Pass Của AD, HRM là 6 số đầu của điện thoại (nếu không có số điện thoại, pass là Hpl@123)<br />";
                MailHelper.EmailSender(userListEmail);
            }
        }
    }
}