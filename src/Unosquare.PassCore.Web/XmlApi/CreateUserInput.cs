namespace Unosquare.PassCore.Web.XmlApi
{
    public class CreateUserInput
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string AdminNotes { get; set; }
        public string MailList { get; set; }
        public string Group { get; set; }
    }
}