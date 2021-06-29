
namespace Unosquare.PassCore.Web.MdaemonServices
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Threading.Tasks;

    public class MdaemonAuthen
    {
        public static async Task<XElement> GetResponse(XmlDocument xmlDoc)
        {
            //Server HPL
            Uri url = new Uri("https://mail.haiphatland.com.vn:1000/MdMgmtWS");
            //var user1 = "hpltech@2stay.vn";
            var user1 = "Haiphatlandtech@haiphatland.com.vn";
            var pass = "Matkhaumoi1108!%#";

            //Uri url = new Uri("https://172.168.0.217:444/MdMgmtWS");
            ////var domain = "company.test";
            //var user1 = "baonx@company.test";
            ////var user2 = "testapi@company.test";
            //var pass = "Admin@123";
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(user1 + ":" + pass));

            //Sử dụng HttpClient
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);
            client.BaseAddress = url;
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", svcCredentials);

            var httpContent = new StringContent(xmlDoc.InnerXml, Encoding.UTF8, "text/xml");

            var respone = await client.PostAsync(url, httpContent);
            string resultContent = respone.Content.ReadAsStringAsync().Result;
            XElement incomingXml2 = XElement.Parse(resultContent);

            return incomingXml2;
        }

        public static string GetXmlFile(string xmlFileName)
        {
            return Directory.GetCurrentDirectory() + "/XmlApi/" + xmlFileName + ".xml";
        }

        //public static async Task<ApiResult> GetUserInfo()
        //{
        //    ApiResult result = new ApiResult();

        //    Uri url = new Uri("https://172.168.0.217:444/MdMgmtWS");
        //    var xmlFile = Directory.GetCurrentDirectory() + "/XmlApi/GetUserInfo.xml";
        //    var domain = "company.test";
        //    var user1 = "baonx@company.test";
        //    var user2 = "testapi@company.test";
        //    var pass = "Admin@123";
        //    //var abc= objHttp.setRequestHeader("Authorization", "Basic " + Base64.("charles.xavier@x-men.int:Password"));
        //    string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(user1 + ":" + pass));

        //    //Sử dụng HttpClient
        //    HttpClientHandler clientHandler = new HttpClientHandler();
        //    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        //    HttpClient client = new HttpClient(clientHandler);
        //    client.BaseAddress = url;
        //    client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Basic", svcCredentials);

        //    try
        //    {
        //        XmlDocument docRequest = new XmlDocument();
        //        docRequest.Load(xmlFile);
        //        var httpContent = new StringContent(docRequest.InnerXml, Encoding.UTF8, "text/xml");

        //        var respone = await client.PostAsync(url, httpContent);
        //        //XmlDocument xmlRes = new XmlDocument();
        //        //xmlRes.LoadXml(respone.Content.ReadAsStringAsync().Result);
        //        //result.Payload = JsonConvert.SerializeXmlNode(xmlRes);
        //        string resultContent = respone.Content.ReadAsStringAsync().Result;
        //        //XDocument incomingXml = XDocument.Load(resultContent);
        //        XElement incomingXml2 = XElement.Parse(resultContent);
        //        //XmlNode xmlNode = new XmlDocument(incomingXml);

        //        result.Payload = incomingXml2;
        //    }
        //    catch (Exception e)
        //    {
        //        result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Error: " + e.Message));
        //    }

        //    return result;
        //    //Sử dụng WebRequest và HttpWebRequest
        //    //var request = (HttpWebRequest)WebRequest.Create(url);
        //    //request.Headers.Add("Authorization", "Basic " + svcCredentials
        //    //
        //    //The remote certificate is invalid according to the validation procedure: RemoteCertificateNameMismatch, RemoteCertificateChainErrors
        //}
    }
}