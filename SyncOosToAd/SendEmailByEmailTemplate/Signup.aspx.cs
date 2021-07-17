using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

//System.Net
using System.Net;

//System.Net.Mail namespace required to send mail.
using System.Net.Mail;

using System.Configuration;
using System.IO;

public partial class Signup : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        //Fetching Settings from WEB.CONFIG file.
        string emailSender = ConfigurationManager.AppSettings["emailsender"].ToString();
        string emailSenderPassword = ConfigurationManager.AppSettings["password"].ToString();
        string emailSenderHost = ConfigurationManager.AppSettings["smtpserver"].ToString();
        int emailSenderPort = Convert.ToInt16(ConfigurationManager.AppSettings["portnumber"]);
        Boolean emailIsSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSSL"]);


        //Fetching Email Body Text from EmailTemplate File.
        string FilePath = "D:\\MBK\\SendEmailByEmailTemplate\\EmailTemplates\\SignUp.html";
        StreamReader str = new StreamReader(FilePath);
        string MailText = str.ReadToEnd();
        str.Close();

        //Repalce [newusername] = signup user name 
        MailText = MailText.Replace("[newusername]", txtUserName.Text.Trim());
               

        string subject = "Welcome to CSharpCorner.Com";

        //Base class for sending email
        MailMessage _mailmsg = new MailMessage();

        //Make TRUE because our body text is html
        _mailmsg.IsBodyHtml = true;

        //Set From Email ID
        _mailmsg.From = new MailAddress(emailSender);

        //Set To Email ID
        _mailmsg.To.Add(txtUserName.Text.ToString());

        //Set Subject
        _mailmsg.Subject = subject;

        //Set Body Text of Email 
        _mailmsg.Body = MailText;

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
}