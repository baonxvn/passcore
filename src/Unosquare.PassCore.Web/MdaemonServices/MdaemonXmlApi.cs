
namespace Unosquare.PassCore.Web.MdaemonServices
{
    using System;
    using System.Threading.Tasks;
    using System.Xml;
    using Models;
    using Common;
    using System.Collections.Generic;
    using Hpl.HrmDatabase.Services;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static MdaemonAuthen;
    using System.Linq;
    using Hpl.HrmDatabase.ViewModels;
    using Hpl.HrmDatabase;

    public class MdaemonXmlApi
    {
        //public const string Domain = "company.test";
        public const string Domain = "haiphatland.com.vn";
        public const string PwDefault = "Hpl@123";

        private static string _paraNode = "/MDaemon/API/Request/Parameters";
        public static async Task<ApiResult> GetUserInfo(string username)
        {
            ApiResult result = new ApiResult();

            var xmlFile = GetXmlFile(new string("GetUserInfo"));

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(_paraNode + "/Domain")!.InnerText = Domain;
                xmlDoc.SelectSingleNode(_paraNode + "/Mailbox")!.InnerText = username;

                result.Payload = await GetResponse(xmlDoc);
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static string GetNewUserFromMdaemon(string userNameCheck)
        {
            string userName = userNameCheck;
            int i = 0;
            bool check = true;
            while (check)
            {
                var res = GetUserInfo(userName);
                var payload = res.Result.Payload.ToString();

                if (payload != null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(payload);
                    var node = xmlDoc.SelectSingleNode("/MDaemon/API/Response/Result");
                    if (node != null)
                    {
                        i++;
                        userName = userNameCheck + i;
                    }
                    else
                    {
                        check = false;
                    }
                }
                else
                {
                    check = false;
                }
            }

            if (i == 0) return userName;

            return userName;
        }

        public static async Task<ApiResult> CreateUserTheoMaNhanVien(string maNhanVien)
        {
            var dbAbp = new AbpHplDbContext();
            ApiResult result = new ApiResult();
            CreateUserInput inputMail = new CreateUserInput();
            inputMail.AdminNotes = "Add by Tools. Time: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
            inputMail.Domain = Domain;
            //MailList { get; set; }
            //Group { get; set; }

            //Lấy thông tin hồ sơ nhân viên theo mã nhân viên
            var listNvs = UserService.GetAllNhanVienTheoMa(maNhanVien);
            switch (listNvs.Count)
            {
                case > 1:
                    string ids = "";
                    foreach (var model in listNvs)
                    {
                        ids += model.NhanVienID + ", ";
                    }
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic,
                        "Lỗi: Mã nhân viên này được sử dụng cho nhiều hồ sơ. Có các ID: " + ids));
                    return result;
                case < 1:
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Không tìm thấy hồ sơ nhân sự."));
                    return result;
            }

            var nhanVien = listNvs.FirstOrDefault();
            if (nhanVien == null)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Không tìm thấy hồ sơ nhân sự."));
                return result;
            }
            inputMail.FullName = nhanVien.Ho + " " + nhanVien.Ten;
            inputMail.FirstName = nhanVien.Ten;
            inputMail.LastName = nhanVien.Ho;

            //Tạo username dựa trên Họ Và Tên nhân sự
            var ho = CommonHelper.ConvertToUnSign(nhanVien.Ho.Trim()
                                    .Replace(" ", ",")
                                    .Replace(",,", ","))
                                    .Split(",");
            string newHo = "";
            foreach (var s in ho)
            {
                newHo += s.Substring(0, 1);
            }
            var ten = CommonHelper.ConvertToUnSign(nhanVien.Ten.Trim().ToLower());
            string userName = ten + newHo.ToLower();

            //Get user trên MDaemon
            inputMail.Username = GetNewUserFromMdaemon(userName);
            inputMail.Password = PwDefault;

            //Xác định phòng, ban cấp 1 của Hồ sơ nhân sự dựa vào Mã Nhân viên
            PhongBan phongBan = UserService.GetPhongBanCap1CuaNhanVien(maNhanVien);
            if (phongBan == null)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Mã nhân sự này không xác định được hồ sơ nhân sự."));
                return result;
            };
            //Xác định mail group cho nhân sự này
            HplPhongBan? hplPhong = dbAbp.HplPhongBans.FirstOrDefault(x => x.PhongBanId == nhanVien.PhongBanId);
            if (hplPhong == null)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Trong cài đặt mapping phòng ban, không xác định được Phòng ban tương ứng. Mã nhân viên: " + maNhanVien));
                return result;
            }

            //Check server AD
            //TODO

            //Tạo email trên MDaemon
            var xmlFile = GetXmlFile(new string("CreateUser"));
            List<object> listObj = new List<object>();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(_paraNode + "/Domain")!.InnerText = inputMail.Domain;
                xmlDoc.SelectSingleNode(_paraNode + "/Mailbox")!.InnerText = inputMail.Username;

                var xmlNodeDetail = (XmlElement)xmlDoc.SelectSingleNode(_paraNode + "/Details")!;

                XmlElement password = xmlDoc.CreateElement("Password");
                password.InnerText = inputMail.Password;
                xmlNodeDetail.AppendChild(password);

                XmlElement adminNotes = xmlDoc.CreateElement("AdminNotes");
                adminNotes.InnerText = inputMail.AdminNotes;
                xmlNodeDetail.AppendChild(adminNotes);

                XmlElement firstName = xmlDoc.CreateElement("FirstName");
                firstName.InnerText = inputMail.FirstName;
                xmlNodeDetail.AppendChild(firstName);

                XmlElement lastName = xmlDoc.CreateElement("LastName");
                lastName.InnerText = inputMail.LastName;
                xmlNodeDetail.AppendChild(lastName);

                XmlElement fullName = xmlDoc.CreateElement("FullName");
                fullName.InnerText = inputMail.FullName;
                xmlNodeDetail.AppendChild(fullName);

                xmlDoc.DocumentElement?.AppendChild(xmlNodeDetail);

                //Add Mailing list;
                //Mặc định add vào all@haiphatland.com.vn
                var xmlNodeListAll = (XmlElement)xmlDoc.SelectSingleNode(_paraNode + "/ListMembership")!;
                XmlElement mailAll = xmlDoc.CreateElement("List");
                mailAll.InnerText = "all@haiphatland.com.vn";
                xmlNodeListAll.AppendChild(mailAll);
                xmlDoc.DocumentElement?.AppendChild(xmlNodeDetail);

                //Add vào Mail list của Phòng Ban
                var xmlNodeListPhongBan = (XmlElement)xmlDoc.SelectSingleNode(_paraNode + "/ListMembership")!;
                XmlElement mailPhongBan = xmlDoc.CreateElement("List");
                mailPhongBan.InnerText = inputMail.MailList;
                xmlNodeListPhongBan.AppendChild(mailPhongBan);
                xmlDoc.DocumentElement?.AppendChild(xmlNodeListPhongBan);

                listObj.Add(await GetResponse(xmlDoc));

                result.Payload = listObj;
            }
            catch (Exception e)
            {
                result.Errors.Add(
                    new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message +
                                                           ". Lưu ý: Kiểm tra lại Mail nhóm đã có trên MDaemon chưa?"));
            }

            return result;
        }

        public static async Task<ApiResult> CreateUser(List<CreateUserInput> listUser)
        {
            ApiResult result = new ApiResult();

            var xmlFile = GetXmlFile(new string("CreateUser"));
            List<object> listObj = new List<object>();

            try
            {
                foreach (var input in listUser)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlFile);

                    xmlDoc.SelectSingleNode(_paraNode + "/Domain")!.InnerText = Domain;
                    xmlDoc.SelectSingleNode(_paraNode + "/Mailbox")!.InnerText = input.Username;

                    var xmlNode = (XmlElement)xmlDoc.SelectSingleNode(_paraNode + "/Details")!;

                    XmlElement password = xmlDoc.CreateElement("Password");
                    password.InnerText = input.Password;
                    xmlNode.AppendChild(password);

                    XmlElement adminNotes = xmlDoc.CreateElement("AdminNotes");
                    adminNotes.InnerText = input.AdminNotes;
                    xmlNode.AppendChild(adminNotes);

                    XmlElement firstName = xmlDoc.CreateElement("FirstName");
                    firstName.InnerText = input.FirstName;
                    xmlNode.AppendChild(firstName);

                    XmlElement lastName = xmlDoc.CreateElement("LastName");
                    lastName.InnerText = input.LastName;
                    xmlNode.AppendChild(lastName);

                    XmlElement fullName = xmlDoc.CreateElement("FullName");
                    fullName.InnerText = input.FullName;
                    xmlNode.AppendChild(fullName);

                    xmlDoc.DocumentElement?.AppendChild(xmlNode);

                    listObj.Add(await GetResponse(xmlDoc));
                }

                result.Payload = listObj;
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static async Task<ApiResult> GetDomainList()
        {
            ApiResult result = new ApiResult();

            try
            {
                var xmlFile = GetXmlFile(new string("GetDomainList"));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);
                result.Payload = await GetResponse(xmlDoc);
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static async Task<ApiResult> MailingGetListInfo(string listName)
        {
            ApiResult result = new ApiResult();

            try
            {
                var xmlFile = GetXmlFile(new string("MailingGetListInfo"));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(_paraNode + "/Domain")!.InnerText = Domain;
                xmlDoc.SelectSingleNode(_paraNode + "/ListName")!.InnerText = listName;

                result.Payload = await GetResponse(xmlDoc);
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static async Task<ApiResult> MailingListCountUsers(string listName)
        {
            ApiResult result = new ApiResult();

            try
            {
                var xmlFile = GetXmlFile(new string("MailingGetListInfo"));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(_paraNode + "/Domain")!.InnerText = Domain;
                xmlDoc.SelectSingleNode(_paraNode + "/ListName")!.InnerText = listName;

                var res = await GetResponse(xmlDoc);
                //XmlDocument xdoc = new XmlDocument();
                //xdoc.Load(res);
                //var node = xmlDoc.SelectSingleNode();MDaemon
                var count = res.Element("API")
                                .Element("Response")
                                .Element("Result")
                                .Element("List")
                                .Element("Members")
                                .FirstAttribute.Value;

                result.Payload = count;

                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Success, "Successful"));
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static async Task<ApiResult> MailingCreateList(string listName, string description)
        {
            ApiResult result = new ApiResult();

            try
            {
                var xmlFile = GetXmlFile(new string("MailingCreateList"));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(_paraNode + "/Domain")!.InnerText = Domain;
                xmlDoc.SelectSingleNode(_paraNode + "/ListName")!.InnerText = listName;
                xmlDoc.SelectSingleNode(_paraNode + "/Details/Description")!.InnerText = listName;

                result.Payload = await GetResponse(xmlDoc);
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static async Task<ApiResult> MailingUpdateList(string listName, string description)
        {
            ApiResult result = new ApiResult();

            try
            {
                var xmlFile = GetXmlFile(new string("MailingUpdateList"));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(_paraNode + "/Domain")!.InnerText = Domain;
                xmlDoc.SelectSingleNode(_paraNode + "/ListName")!.InnerText = listName;
                xmlDoc.SelectSingleNode(_paraNode + "/Details/Description")!.InnerText = description;

                //Cập nhật danh sách user ở đây
                var xmlNode = (XmlElement)xmlDoc.SelectSingleNode(_paraNode + "/Members");

                XmlElement member1 = xmlDoc.CreateElement("Member");
                member1.SetAttribute("action", "add");
                member1.SetAttribute("id", "baonx@company.test");
                member1.SetAttribute("displayname", "Nguyen Xuan Bao");
                xmlNode.AppendChild(member1);
                xmlDoc.DocumentElement.AppendChild(xmlNode);

                XmlElement member2 = xmlDoc.CreateElement("Member");
                member2.SetAttribute("action", "add");
                member2.SetAttribute("id", "baonx2@company.test");
                member2.SetAttribute("displayname", "Nguyen Xuan Bao2");

                xmlNode.AppendChild(member2);
                xmlDoc.DocumentElement.AppendChild(xmlNode);

                result.Payload = await GetResponse(xmlDoc);
                result.Payload = xmlDoc;
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }
    }
}