using System.Linq;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Hpl.HrmDatabase.Services
{
    public static class MyStringExtensions
    {
        public static bool Like(this string toSearch, string toFind)
        {
            return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(toSearch);
        }
    }

    public class UsernameGenerator
    {
        public static bool LikeString(string toSearch, string toFind)
        {
            return toSearch.Like("%" + toFind);
        }

        public static string ConvertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string CreateUsernameFromName(string lastName, string firstName)
        {
            //Tạo username dựa trên Họ Và Tên nhân sự
            var ho = ConvertToUnSign(lastName.Trim()
                    .Replace(" ", ",")
                    .Replace(",,", ","))
                .Split(",");
            string newHo = "";
            foreach (var s in ho)
            {
                newHo += s.Substring(0, 1);
            }
            var ten = ConvertToUnSign(firstName.Trim().ToLower());
            string userNameGenerated = ten + newHo.ToLower();

            return userNameGenerated;
        }

        public static string CreateNewUsername(string userName)
        {
            var db = new HrmDbContext();
            string newUsername = userName;
            bool check = true;
            int i = 0;
            while (check)
            {
                var user = db.SysNguoiDungs.FirstOrDefault(x => x.TenDangNhap == newUsername);
                if (user != null)
                {
                    i++;
                    newUsername = userName + i;
                }
                else
                {
                    check = false;
                }

            }

            return newUsername;
        }

        //public static string CreateUsername(string hoVaTen)
        //{
        //    //Tạo username dựa trên Họ Và Tên nhân sự
        //    var str1 = CommonHelper.ConvertToUnSign(hoVaTen.Trim()
        //            .Replace(" ", ",")
        //            .Replace(",,", ","))
        //        .Split(",");

        //    var ten = str1.LastOrDefault();

        //    string newHo = "";
        //    foreach (var s in str1)
        //    {
        //        newHo += s.Substring(0, 1);
        //    }
        //    var ten = CommonHelper.ConvertToUnSign(firstName.Trim().ToLower());
        //    string userNameGenerated = ten + newHo.ToLower();

        //    return userNameGenerated;
        //}
    }
}