using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hpl.HrmDatabase;
using Hpl.HrmDatabase.Services;
using Hpl.HrmDatabase.ViewModels;
using Hpl.SaleOnlineDatabase;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;
using Unosquare.PassCore.Web.MdaemonServices;
using NhanVienSale = Hpl.SaleOnlineDatabase.NhanVien;
using PhongBan = Hpl.HrmDatabase.PhongBan;

namespace Hpl.Common
{
    public class HplServices
    {
        private static PasswordChangeOptions _options;
        private readonly ILogger _logger;
        private readonly IPasswordChangeProvider _passwordChangeProvider;

        public HplServices(IPasswordChangeProvider passwordChangeProvider, PasswordChangeOptions options, ILogger logger)
        {
            _passwordChangeProvider = passwordChangeProvider;
            _logger = logger;
            _options = options;
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Information()
            //    .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //        (utcDateTime, wt) => wt.File($"logs/acm-log-{utcDateTime}.txt"))
            //    .CreateLogger();
            //Log.Information("----START HAI PHAT LAND ACM----");
        }

        public async Task CreateUserAllSys(List<NhanVienViewModel> listNvs)
        {
            string listUserBodyEmail = "<p style=\"font-weight: bold\">DANH SÁCH USER ĐÃ TẠO MỚI HOẶC SỬA THÔNG TIN</p>";
            listUserBodyEmail += "<p><a href=\"https://acm.haiphatland.com.vn/\">https://acm.haiphatland.com.vn/</a></p>";

            string userSale = "<p style=\"font-weight: bold\">DANH SÁCH USER ĐÃ TẠO SALE ONLINE</p>";
            int i = 0;
            int j = 0;
            List<HplSyncLog> listLog = new List<HplSyncLog>();
            List<HplNhanVienLog> listNvLogs = new List<HplNhanVienLog>();
            List<EmailNotifications> listNotifications = new List<EmailNotifications>();
            List<string> listEmailLoi = new List<string>();

            foreach (var model in listNvs)
            {
                //if (!model.MaNhanVien.Equals("KD8-332")) continue;
                try
                {
                    i++;
                    string userName = UsernameGenerator.CreateUsernameFromName(model.Ho, model.Ten);
                    PhongBan phongBanCap1 = UserService.GetPhongBanCap1CuaNhanVien(model.MaNhanVien);
                    var abpPhong = AbpServices.GetAbpPhongBanByMaPhongBan(phongBanCap1.MaPhongBan);

                    string tenPhongBan = "HAI PHAT LAND COMPANY";
                    if (phongBanCap1 != null)
                    {
                        tenPhongBan = phongBanCap1.Ten;
                    }
                    else
                    {
                        listUserBodyEmail =
                            "<p style=\"font-weight: bold\">LỖI: NHÂN VIÊN " + model.Ho + " " + model.Ten +
                            " (" + model.MaNhanVien + ") KHÔNG XÁC ĐỊNH ĐƯỢC PHÒNG BAN CẤP 1</p>";
                        MailHelper.EmailSender(listUserBodyEmail);
                        return;
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
                    _logger.Information(userName + " CREATED on HRM at " + DateTime.Now.ToString("G"));
                    Console.WriteLine(userName + " CREATED on HRM at " + DateTime.Now.ToString("G"));

                    //TẠO EMAIL
                    string mailList = "";
                    if (abpPhong != null)
                    {
                        if (string.IsNullOrEmpty(abpPhong.MailingList))
                        {
                            mailList = abpPhong.MailingList;
                        }
                    }
                    CreateUserInput input = new CreateUserInput
                    {
                        Domain = "haiphatland.com.vn",
                        Username = userName,
                        FirstName = ten,
                        LastName = ho,
                        FullName = hoVaTen,
                        Password = "Hpl@123",
                        AdminNotes = "Tạo từ tool, time: " + DateTime.Now.ToString("G"),
                        MailList = mailList,
                        Group = ""
                    };
                    var res = await MdaemonXmlApi.CreateUser(input);

                    _logger.Information(userName + " CREATED on MDaemon at " + DateTime.Now.ToString("G"));
                    Console.WriteLine(userName + " CREATED on MDaemon at " + DateTime.Now.ToString("G"));

                    //Update log vào DB
                    HplSyncLog syncLog = new HplSyncLog
                    {
                        UserName = userName,
                        MaNhanVien = model.MaNhanVien,
                        Payload = JsonConvert.SerializeObject(nhanVien),
                        LogForSys = "HRM"
                    };
                    listLog.Add(syncLog);

                    //TẠO USER TRÊN SALEONLINE
                    string isSaleOnline = "Đã tồn tại";
                    var nvSale = SaleOnlineServices.GetNhanVienByUserName(userName);
                    if (nvSale != null)
                    {
                        //CẬP NHẬT LẠI TRẠNG THÁI USER
                        //Check theo mã Nhân Viên
                        if (nvSale.KeyCode == model.MaNhanVien)
                        {
                            if (nvSale.Lock.Value)
                            {
                                nvSale.Lock = false;
                                isSaleOnline = "Đã unlock user";
                            }

                            //Xác định BranchId trên SaleOnline
                            var branch = SaleOnlineServices.GetBranchId(phongBanCap1.MaPhongBan);
                            if (branch != null)
                            {
                                j++;
                                if (nvSale.BranchId != branch.BranchId)
                                {
                                    nvSale.BranchId = branch.BranchId;
                                    isSaleOnline = "Đã cập nhật BranchID";
                                }
                            }

                            var syncLogSale = new HplSyncLog
                            {
                                UserName = userName,
                                MaNhanVien = model.MaNhanVien,
                                Payload = JsonConvert.SerializeObject(nvSale),
                                LogForSys = "SaleOnline"
                            };
                            //Cập nhật user
                            SaleOnlineServices.UpdateUserSale(nvSale);

                            listLog.Add(syncLogSale);

                            userSale += j + ". " + model.Ho + " " + model.Ten + " - " +
                                        model.MaNhanVien + " - " + userName + "<br />";

                            _logger.Information(userName + " UPDATED on SaleOnline at " + DateTime.Now.ToString("G"));
                            Console.WriteLine(userName + " UPDATED on SaleOnline at " + DateTime.Now.ToString("G"));
                        }
                        else
                        {
                            //Add số 1 vào user này và cập nhật
                            nvSale.MaSo = "1" + nvSale.MaSo;
                            SaleOnlineServices.UpdateUserSale(nvSale);
                            //Và tạo mới user khác.
                            //TẠO MỚI
                            nvSale = new NhanVienSale
                            {
                                MaSo = userName,
                                //nvSale.MatKhau = model.
                                HoTen = model.Ho + " " + model.Ten,
                                Ho = model.Ho,
                                Ten = model.Ten,
                                DienThoai = model.DienThoai,
                                Email = nhanVien.Email,
                                //nvSale.NgaySinh = model.
                                UserType = 1,
                                Lock = false,
                                SoCmnd = model.CMTND,
                                KeyCode = model.MaNhanVien,
                                MaNvcn = 8,
                                IsDeleted = false
                            };

                            //Xác định BranchId trên SaleOnline
                            var branch = SaleOnlineServices.GetBranchId(phongBanCap1.MaPhongBan);
                            if (branch != null)
                            {
                                j++;
                                nvSale.BranchId = branch.BranchId;
                                SaleOnlineServices.CreateUserSale(nvSale);

                                var syncLogSale = new HplSyncLog
                                {
                                    UserName = userName,
                                    MaNhanVien = model.MaNhanVien,
                                    Payload = JsonConvert.SerializeObject(nvSale),
                                    LogForSys = "SaleOnline"
                                };

                                isSaleOnline = "Đã tạo lại";
                                listLog.Add(syncLogSale);

                                userSale += j + ". " + model.Ho + " " + model.Ten + " - " +
                                            model.MaNhanVien + " - " + userName +
                                            " (" + phongBanCap1.MaPhongBan + ")<br />";

                                _logger.Information(userName + " UPDATED & CREATED on SaleOnline at " + DateTime.Now.ToString("G"));
                                Console.WriteLine(userName + " UPDATED & CREATED on SaleOnline at " + DateTime.Now.ToString("G"));
                            }
                        }
                    }
                    else
                    {
                        //TẠO MỚI
                        nvSale = new NhanVienSale();
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
                        var branch = SaleOnlineServices.GetBranchId(phongBanCap1.MaPhongBan);
                        if (branch != null)
                        {
                            j++;
                            nvSale.BranchId = branch.BranchId;
                            SaleOnlineServices.CreateUserSale(nvSale);

                            var syncLogSale = new HplSyncLog
                            {
                                UserName = userName,
                                MaNhanVien = model.MaNhanVien,
                                Payload = JsonConvert.SerializeObject(nvSale),
                                LogForSys = "SaleOnline"
                            };

                            isSaleOnline = "Đã tạo";
                            listLog.Add(syncLogSale);

                            userSale += j + ". " + model.Ho + " " + model.Ten + " - ";
                            userSale += model.MaNhanVien + " - " + userName + "<br />";

                            _logger.Information(userName + " CREATED on SaleOnline at " + DateTime.Now.ToString("G"));
                            Console.WriteLine(userName + " CREATED on SaleOnline at " + DateTime.Now.ToString("G"));
                        }
                    }

                    listUserBodyEmail += i + ". <a href=\"https://hrm.haiphatland.com.vn/HRIS/Profile/Index/" + model.NhanVienID + "\">";
                    listUserBodyEmail += model.Ho + " " + model.Ten + "</a>. Mã NV: ";
                    listUserBodyEmail += model.MaNhanVien + ". User: " + userInfo.sAMAccountName;
                    listUserBodyEmail += " (" + phongBanCap1.MaPhongBan + ")<br />";

                    //ADD LIST GỬI THEO TỪNG BỘ PHẬN
                    var emailNoti = listNotifications.FirstOrDefault(x => x.MaPhongBanCap1 == phongBanCap1.MaPhongBan);

                    if (emailNoti == null)
                    {
                        emailNoti = new EmailNotifications();
                        emailNoti.MaPhongBanCap1 = phongBanCap1.MaPhongBan;
                        emailNoti.TenPhongBanCap1 = phongBanCap1.Ten;
                        if (!string.IsNullOrEmpty(abpPhong.EmailNotification))
                        {
                            emailNoti.EmailNotifyReceiver = abpPhong.EmailNotification;
                        }
                        else
                        {
                            listEmailLoi.Add(phongBanCap1.Ten + " chưa cập nhật email trợ lý vào ACM");
                        }

                        emailNoti.ListUsers = new List<string>();

                        listNotifications.Add(emailNoti);
                    }

                    //TODO for testing
                    //emailNoti.ListUsers.Add("<a href=\"https://hrm.haiphatland.com.vn/HRIS/Profile/Index/" +
                    //                        model.NhanVienID + "\">" + model.Ho + " " + model.Ten +
                    //                        "</a>. Mã NV: " +
                    //                        model.MaNhanVien + ". User: <br />");

                    emailNoti.ListUsers.Add("<a href=\"https://hrm.haiphatland.com.vn/HRIS/Profile/Index/" +
                                            model.NhanVienID + "\">" + model.Ho + " " + model.Ten +
                                            "</a>. Mã NV: " +
                                            model.MaNhanVien + ". User: " + userInfo.sAMAccountName +
                                            " (" + phongBanCap1.MaPhongBan + ")<br />");

                    //Add new LogNhanVien
                    var nvLog = new HplNhanVienLog
                    {
                        NhanVienId = model.NhanVienID,
                        FirstName = model.Ten,
                        LastName = model.Ho,
                        GioiTinh = model.GioiTinh,
                        MaNhanVien = model.MaNhanVien,
                        TenDangNhap = userName,
                        Email = nhanVien.Email,
                        EmailCaNhan = nhanVien.EmailCaNhan,
                        DienThoai = nhanVien.DienThoai,
                        Cmtnd = nhanVien.CMTND,
                        TenChucVu = nhanVien.TenChucVu,
                        TenChucDanh = nhanVien.TenChucDanh,
                        PhongBanId = nhanVien.PhongBanId,
                        TenPhongBan = nhanVien.TenPhongBan,
                        PhongBanCap1Id = phongBanCap1.PhongBanId,
                        TenPhongBanCap1 = phongBanCap1.Ten,
                        IsAd = "OK",
                        IsHrm = "OK",
                        IsSaleOnline = isSaleOnline,
                        IsEmail = "OK",
                        LinkHrm = "https://hrm.haiphatland.com.vn/HRIS/Profile/Index/" + nhanVien.NhanVienID + "/",
                        LinkEmail = "https://id.haiphatland.com.vn:86/api/mdaemon/GetUserInfo?username=" + userName,
                        DateCreated = DateTime.Now
                    };

                    listNvLogs.Add(nvLog);
                }
                catch (Exception e)
                {
                    _logger.Error("Error create user for MaNhanVien: " + model.MaNhanVien +
                                  ". Errors: " + e);
                    Console.WriteLine("Error create user for MaNhanVien: " + model.MaNhanVien +
                                      ". Errors: " + e);
                }
            }

            if (listLog.Any())
            {
                AbpServices.AddSyncLogAbp(listLog);
            }

            if (listNvLogs.Any())
            {
                AbpServices.AddLogNhanVien(listNvLogs);
            }

            if (listNvs.Any())
            {
                //GỬI MAIL CHO ADMIN
                if (j > 0) listUserBodyEmail += userSale;

                listUserBodyEmail += "Lưu ý:<br />" +
                                 "1. Pass mặc định của mail là Hpl@123<br />" +
                                 "2. Pass Của HRM và SaleOnline là 6 số đầu của điện thoại (nếu không có số điện thoại, pass là Hpl@123)<br />";
                MailHelper.EmailSender(listUserBodyEmail);

                if (listNotifications.Any())
                {
                    //GỬI MAIL CHO CÁC TRỢ LÝ
                    foreach (var notification in listNotifications)
                    {
                        List<string> listReceivers = new List<string>();

                        int l = 0;
                        if (!string.IsNullOrEmpty(notification.EmailNotifyReceiver))
                        {
                            var listEmails = notification.EmailNotifyReceiver.Split(",");
                            foreach (var email in listEmails)
                            {
                                if (MailHelper.IsValidEmail(email))
                                {
                                    listReceivers.Add(email);
                                }
                            }
                            //Danh sách thông tin tài khoản đã tạo
                            string bodyEmail = "<p style=\"font-weight: bold\">DANH SÁCH TÀI KHOẢN ĐÃ TẠO</p>";
                            bodyEmail += "<p style=\"font-weight: bold\">" + notification.TenPhongBanCap1 + "</p>";
                            foreach (var user in notification.ListUsers)
                            {
                                l++;
                                bodyEmail += l + ". " + user;
                            }

                            bodyEmail += "Lưu ý:<br />";
                            bodyEmail += "1. Pass mặc định của mail là Hpl@123<br />";
                            bodyEmail += "2. Pass Của HRM và SaleOnline là 6 số đầu của điện thoại (nếu không có số điện thoại, pass là Hpl@123)<br />";

                            MailHelper.EmailSender(bodyEmail, "[HPL] HCNS THÔNG BÁO TÀI KHOẢN ĐÃ TẠO NGÀY " + DateTime.Now.ToString("dd/MM/yyyy"), listReceivers);
                        }
                    }

                    //GỬI MAIL THÔNG BÁO ADMIN CÁC EMAIL TRỢ LÝ KHÔNG ĐÚNG.
                    if (listEmailLoi.Any())
                    {
                        string bodyEmail = "<p style=\"font-weight: bold\">DANH SÁCH EMAIL TRỢ LÝ KHÔNG ĐÚNG</p>";
                        foreach (var str in listEmailLoi)
                        {
                            bodyEmail += str + "<br />";
                        }
                        MailHelper.EmailSenderAdmin(bodyEmail, "[HPL] LỖI KHÔNG GỬI ĐƯỢC EMAIL");
                    }
                }
            }
        }
    }
}