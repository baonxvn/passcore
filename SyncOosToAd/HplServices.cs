using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hpl.HrmDatabase;
using Hpl.HrmDatabase.Services;
using Hpl.HrmDatabase.ViewModels;
using Newtonsoft.Json;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.Web.MdaemonServices;

namespace SyncOosToAd
{
    public class HplServices
    {
        private readonly IPasswordChangeProvider _passwordChangeProvider;

        public HplServices()
        {

        }

        public HplServices(IPasswordChangeProvider passwordChangeProvider)
        {
            _passwordChangeProvider = passwordChangeProvider;
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
                        Log.Error(model.MaPhongBan + " Số điện thoại lỗi " + e.Message);
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
                    var userInfoAd = _passwordChangeProvider.CreateAdUser(userAd, pw);
                    var userInfo = userInfoAd.UserInfo;

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
    }
}