namespace Unosquare.PassCore.Web.MdaemonServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using Hpl.HrmDatabase.Services;
    using Unosquare.PassCore.Common;
    using Unosquare.PassCore.Web.Models;

    using static MdaemonAuthen;

    public class MdaemonXmlApi
    {
        //public const string MailDomain = "company.test";
        public const string MailDomain = "haiphatland.com.vn";
        private const string ParaNode = "/MDaemon/API/Request/Parameters";

        /// <summary>
        /// Tạo đồng loạt email do lỗi tool chưa được tạo (danh sách user lấy trong bảng HPL_ACM.HplNhanVienLogs)
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResult> FixEmailChuaDuocTao()
        {
            ApiResult result = new ApiResult();
            List<object> lstOut = new List<object>();
            var listLog = AbpServices.GetAllLogNhanVien();
            result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Tổng user: " + listLog.Count));
            foreach (var log in listLog)
            {
                lstOut.Add(await CreateUserByUserName(log.TenDangNhap));
                UserService.UpdateEmailByUserName(log.TenDangNhap);
            }

            result.Payload = lstOut;

            return result;
        }

        public static async Task<ApiResult> GetUserInfo(string username)
        {
            ApiResult result = new ApiResult();

            var xmlFile = GetXmlFile(new string("GetUserInfo"));

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = MailDomain;
                xmlDoc.SelectSingleNode(ParaNode + "/Mailbox")!.InnerText = username;

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

        public static async Task<ApiResult> CreateUserByUserName(string userName)
        {
            var result = new ApiResult();
            var listNvs = UserService.GetNhanVienTheoUserName(userName);
            if (listNvs.Count > 1)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Username bị trùng lặp, kiểm tra lại"));
                return result;
            }

            //TẠO EMAIL
            var nv = listNvs.FirstOrDefault();
            if (nv == null)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Không tìm thấy nhân viên có username này."));
                return result;
            }

            string ten = UsernameGenerator.ConvertToUnSign(nv.Ten);
            string ho = UsernameGenerator.ConvertToUnSign(nv.Ho);

            var input = new CreateUserInput
            {
                Domain = "haiphatland.com.vn",
                Username = userName,
                FirstName = ten,
                LastName = ho,
                FullName = ho + " " + ten,
                Password = "Hpl@123",
                AdminNotes = "Tạo từ tool, time: " + DateTime.Now.ToString("G"),
                MailList = "",
                Group = ""
            };

            result.Payload = await MdaemonXmlApi.CreateUser(input);
            result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Thành công."));

            //Cập nhật lại Email cho nhân sự này
            UserService.UpdateEmailByUserName(userName);

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

                    xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = MailDomain;
                    xmlDoc.SelectSingleNode(ParaNode + "/Mailbox")!.InnerText = input.Username;

                    var xmlNode = (XmlElement)xmlDoc.SelectSingleNode(ParaNode + "/Details")!;

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

        public static async Task<ApiResult> CreateUser(CreateUserInput inputMail)
        {
            List<object> listObj = new List<object>();

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xDeclare = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement documentRoot = doc.DocumentElement;
            doc.InsertBefore(xDeclare, documentRoot);
            XmlElement rootEl = (XmlElement)doc.AppendChild(doc.CreateElement("FIXML"));
            XmlElement child1 = (XmlElement)rootEl.AppendChild(doc.CreateElement("Header"));
            XmlElement child2 = (XmlElement)child1.AppendChild(doc.CreateElement("RequestHeader"));

            ApiResult result = new ApiResult();
            var xmlFile = GetXmlFile(new string("CreateUser"));
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = inputMail.Domain;
                xmlDoc.SelectSingleNode(ParaNode + "/Mailbox")!.InnerText = inputMail.Username;

                var xmlNodeDetail = (XmlElement)xmlDoc.SelectSingleNode(ParaNode + "/Details")!;

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

                listObj.Add(await GetResponse(xmlDoc));

                //Add Mailing list; Mặc định add vào all @haiphatland.com.vn
                listObj.Add(await MailingUpdateList("all", inputMail));
                //Add vào Mail list của Phòng Ban
                if (!string.IsNullOrEmpty(inputMail.MailList))
                {
                    listObj.Add(await MailingUpdateList(inputMail.MailList.Split("@")[0], inputMail));
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

                xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = MailDomain;
                xmlDoc.SelectSingleNode(ParaNode + "/ListName")!.InnerText = listName;

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

                xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = MailDomain;
                xmlDoc.SelectSingleNode(ParaNode + "/ListName")!.InnerText = listName;

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

                xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = MailDomain;
                xmlDoc.SelectSingleNode(ParaNode + "/ListName")!.InnerText = listName;
                xmlDoc.SelectSingleNode(ParaNode + "/Details/Description")!.InnerText = listName;

                result.Payload = await GetResponse(xmlDoc);
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static async Task<ApiResult> MailingUpdateList(string listName, List<CreateUserInput> listUser)
        {
            ApiResult result = new ApiResult();

            try
            {
                var xmlFile = GetXmlFile(new string("MailingUpdateList"));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = MailDomain;
                xmlDoc.SelectSingleNode(ParaNode + "/ListName")!.InnerText = listName;
                //xmlDoc.SelectSingleNode(_paraNode + "/Details/Description")!.InnerText = description;

                //Cập nhật danh sách user ở đây
                foreach (var user in listUser)
                {
                    var xmlNode = (XmlElement)xmlDoc.SelectSingleNode(ParaNode + "/Members");

                    XmlElement member1 = xmlDoc.CreateElement("Member");
                    member1.SetAttribute("action", "add");
                    member1.SetAttribute("id", user.Username + "@" + user.Domain);
                    member1.SetAttribute("displayname", user.FullName);
                    xmlNode.AppendChild(member1);
                }

                result.Payload = await GetResponse(xmlDoc);
                result.Payload = xmlDoc;
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
            }

            return result;
        }

        public static async Task<ApiResult> MailingUpdateList(string listName, CreateUserInput user)
        {
            ApiResult result = new ApiResult();

            try
            {
                var xmlFile = GetXmlFile(new string("MailingUpdateList"));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                xmlDoc.SelectSingleNode(ParaNode + "/Domain")!.InnerText = MailDomain;
                xmlDoc.SelectSingleNode(ParaNode + "/ListName")!.InnerText = listName;
                //xmlDoc.SelectSingleNode(_paraNode + "/Details/Description")!.InnerText = description;

                //Cập nhật danh sách user ở đây
                var xmlNode = (XmlElement)xmlDoc.SelectSingleNode(ParaNode + "/Members");

                XmlElement member1 = xmlDoc.CreateElement("Member");
                member1.SetAttribute("action", "add");
                member1.SetAttribute("id", user.Username + "@" + user.Domain);
                member1.SetAttribute("displayname", user.FullName);
                xmlNode.AppendChild(member1);

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