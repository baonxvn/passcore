using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Hpl.Common.Helper
{
    public class CommonHelper
    {
        public static string ConvertStringToPascalCase(string strInput)
        {
            var strOut = strInput.ToLower().Replace(" ", " ");
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            strOut = info.ToTitleCase(strOut).Replace(" ", string.Empty);
            Console.WriteLine(strOut);

            return strOut;
        }

        public static string IsValidEmail(string email)
        {
            try
            {
                email = email.Trim();
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address == email)
                {
                    return addr.Address;
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }

        public static string ConvertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string GenerateUserNameFromFirstNameAndLastName(string firstName, string lastName)
        {
            var ho = ConvertToUnSign(lastName.Trim().ToLower()
                    .Replace(" ", ",")
                    .Replace(",,", ","))
                .Split(",");
            string hoGenerate = "";
            foreach (var s in ho)
            {
                hoGenerate += s.Substring(0, 1);
            }

            var ten = ConvertToUnSign(firstName.Trim().ToLower());
            return ten + hoGenerate;
        }
    }
}