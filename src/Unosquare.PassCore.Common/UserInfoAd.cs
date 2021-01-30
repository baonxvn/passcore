﻿namespace Unosquare.PassCore.Common
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
        /// Get First Name
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string givenName { get; set; }

        /// <summary>
        /// Get Initials
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string initials { get; set; }

        /// <summary>
        /// Get Last Name
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string sn { get; set; }

        /// <summary>
        /// Get Display Name
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string displayName { get; set; }

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
        /// Get Telephone Number
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string telephoneNumber { get; set; }

        /// <summary>
        /// Get Telephone Number (Other)
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string otherTelephone { get; set; }

        /// <summary>
        /// Get mail
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string mail { get; set; }

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
        /// Get Common Name
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string CN { get; set; }

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
    }
}
