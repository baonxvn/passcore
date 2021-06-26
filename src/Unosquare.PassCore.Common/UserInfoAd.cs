namespace Unosquare.PassCore.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Link info  https://docs.secureauth.com/display/KBA/Active+Directory+Attributes+List
    /// </summary>
    public class UserInfoAd
    {
        /// <summary>
        /// Returns a Boolean value that specifies whether the account is currently locked 
        /// </summary>
        /// <value>
        /// true if the account is locked out; otherwise false.
        /// </value>
        public bool isLocked { get; set; }

        /// <summary>
        /// Get 
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string userPrincipalName { get; set; }

        /// <summary>
        /// Get 
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string sAMAccountName { get; set; }

        /// <summary>
        /// name và cn Giống nhau (Rename trong AD thì 2 trường này cùng thay đổi, hiện thị trong rename là Fullname)
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Get Common Name
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string CN { get; set; }

        /// <summary>
        /// Get Last Name Họ (và tên đệm)
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string sn { get; set; }

        /// <summary>
        /// Get First Name Tên
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string givenName { get; set; }

        /// <summary>
        /// Get Initials Tên đệm
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string initials { get; set; }

        /// <summary>
        /// Get Display Name
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string displayName { get; set; }

        /// <summary>
        /// Get mail
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string mail { get; set; }

        /// <summary>
        /// Get Telephone Number (là userPrincipal.VoiceTelephoneNumber trong dll của Microsoft)
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string telephoneNumber { get; set; }

        /// <summary>
        /// Phòng ban user
        /// </summary>
        public string department { get; set; }

        /// <summary>
        /// Chức Danh của user
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public string employeeID { get; set; }

        /// <summary>
        /// Get 
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string description { get; set; }

        /// <summary>
        /// Get Office
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string physicalDeliveryOfficeName { get; set; }

        /// <summary>
        /// Get Telephone Number (Other)
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string otherTelephone { get; set; }

        /// <summary>
        /// Get Home phone
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string homePhone { get; set; }

        /// <summary>
        /// Get mobile phone
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string mobile { get; set; }

        /// <summary>
        /// Get Web Page
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string wWWHomePage { get; set; }

        /// <summary>
        /// Get Web Page (Other)
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string url { get; set; }

        /// <summary>
        /// Get OU CN=baonx,OU=Company Structure,DC=baonx,DC=com
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string distinguishedName { get; set; }

        /// <summary>
        /// Get CN=Employees,CN=Users,DC=baonx,DC=com
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string memberOf { get; set; }

        /// <summary>
        /// Get CN=Person,CN=Schema,CN=Configuration,DC=baonx,DC=com
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string objectCategory { get; set; }
    }
}
