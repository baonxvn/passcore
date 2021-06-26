
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Unosquare.PassCore.Common
{
    public class CommonHelper
    {
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
    }
}