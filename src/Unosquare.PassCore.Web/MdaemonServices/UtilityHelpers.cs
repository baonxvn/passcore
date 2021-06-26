namespace Unosquare.PassCore.Web.MdaemonServices
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Unosquare.PassCore.Web.XmlApi;

    public class UtilityHelpers
    {
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            //const string chars2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //public static List<CreateUserInput> CreateUserTheoMaNhanVien(string maNhanVien)
        //{
        //    var listUsers = listUserName.Trim().Replace(" ", string.Empty).Split(",");

        //    var list = new List<CreateUserInput>();
        //    foreach (var userName in listUserName)
        //    {
        //        var user = new CreateUserInput()
        //        {
        //            Domain = MdaemonXmlApi.Domain,
        //            Username = userName,
        //            FirstName = "FirstName" + i,
        //            LastName = "LastName" + i,
        //            FullName = "FullName" + i,
        //            Password = "password" + i,
        //            AdminNotes = "Tạo bởi tool",
        //            MailList = "bancongnghe@company.test"
        //        };
        //        list.Add(user);
        //    }

        //    return list;
        //}

        public static List<CreateUserInput> CreateUserDemo()
        {
            Random random = new Random();
            int j = random.Next(20);
            var list = new List<CreateUserInput>();
            for (int i = j; i < j + 3; i++)
            {
                var user = new CreateUserInput()
                {
                    Domain = MdaemonXmlApi.Domain,
                    Username = "user" + i,
                    FirstName = "FirstName" + i,
                    LastName = "LastName" + i,
                    FullName = "FullName" + i,
                    Password = "password" + i,
                    AdminNotes = "Tạo bởi tool",
                    MailList = "bancongnghe@company.test"
                };
                list.Add(user);
            }

            return list;
        }
    }
}