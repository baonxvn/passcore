using System.IO;
using System.Net;
using System.Net.Mail;

namespace SyncOosToAd.MyService
{
    public class MailHelper
    {
        public static void EmailSender(string body)
        {
            //Fetching Settings from WEB.CONFIG file.
            string emailSender = "baofsoft@gmail.com";
            string emailSenderPassword = "tadsgszyizelkorl";
            string emailSenderHost = "smtp.gmail.com";
            int emailSenderPort = 587;
            bool emailIsSSL = true;

            //Fetching Email Body Text from EmailTemplate File.
            //string FilePath = "D:\\MBK\\SendEmailByEmailTemplate\\EmailTemplates\\SignUp.html";
            //StreamReader str = new StreamReader(FilePath);
            //string mailText = str.ReadToEnd();
            //str.Close();

            string bodyContent = "Đây là nội dung email";

            //Repalce [newusername] = signup user name 
            //mailText = mailText.Replace("[body_content]", bodyContent);


            string subject = "Thông báo tạo và đồng bộ Tài khoản AD, HRM, MDaemon";

            //Base class for sending email
            MailMessage _mailmsg = new MailMessage();

            //Make TRUE because our body text is html
            _mailmsg.IsBodyHtml = true;

            //Set From Email ID
            _mailmsg.From = new MailAddress(emailSender);

            //Set To Email ID
            _mailmsg.To.Add("baonx@haiphatland.com.vn");
            _mailmsg.To.Add("hoangnq@haiphatland.com.vn");
            _mailmsg.To.Add("daolq@haiphatland.com.vn");

            //Set Subject
            _mailmsg.Subject = subject;

            //Set Body Text of Email 
            //_mailmsg.Body = mailText;
            _mailmsg.Body = body;
            //_mailmsg.Body = bodyContent;

            //Now set your SMTP 
            SmtpClient _smtp = new SmtpClient();

            //Set HOST server SMTP detail
            _smtp.Host = emailSenderHost;

            //Set PORT number of SMTP
            _smtp.Port = emailSenderPort;

            //Set SSL --> True / False
            _smtp.EnableSsl = emailIsSSL;

            //Set Sender UserEmailID, Password
            NetworkCredential _network = new NetworkCredential(emailSender, emailSenderPassword);
            _smtp.Credentials = _network;

            //Send Method will send your MailMessage create above.
            _smtp.Send(_mailmsg);
        }

        public void SendMailToAdmin()
        {
            //tadsgszyizelkorl

            var fromAddress = new MailAddress("baofsoft@gmail.com", "Ban Công Nghệ");
            var toAddress = new MailAddress("to@example.com", "To Name");
            const string fromPassword = "tadsgszyizelkorl";
            const string subject = "Thông báo tạo và đồng bộ Tài khoản AD, HRM, MDaemon";
            const string body = "Body";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }

    class EmailSetting
    {

    }
}