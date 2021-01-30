namespace Unosquare.PassCore.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Unosquare.PassCore.Common;

    public class ApiResultAd
    {
        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public ApiErrorItem? Errors { get; set; }

        /// <summary>
        /// Get user infomation
        /// </summary>
        public UserInfoAd? UserInfo { get; set; }
    }
}
