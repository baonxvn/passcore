﻿
namespace Unosquare.PassCore.Web.Helpers
{
    using System;
    using Common;
    using Serilog;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using Hpl.HrmDatabase.ViewModels;
    using System.Collections.Generic;

    internal class DebugPasswordChangeProvider : IPasswordChangeProvider
    {
        private readonly ILogger _logger;

        public DebugPasswordChangeProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ApiResultAd? UpdateUserInfo(string username)
        {
            throw new NotImplementedException();
        }

        public string GetUserNewUserFormAd(string username)
        {
            throw new NotImplementedException();
        }

        public ApiResultAd CreateAdUser(UserInfoAd user, string pw)
        {
            throw new NotImplementedException();
        }

        public List<ApiResultAd> UpdateUserInfo(List<NhanVienViewModel> listNvs)
        {
            throw new NotImplementedException();
        }

        public ApiErrorItem? PerformPasswordChange(string username, string currentPassword, string newPassword)
        {
            _logger.Information("DebugPasswordChangeProvider.PerformPasswordChange: username=" + username +
                    ". currentPassword=" + currentPassword +
                    ". newPassword=" + newPassword);

            var currentUsername = username.IndexOf("@", StringComparison.Ordinal) > 0
                ? username.Substring(0, username.IndexOf("@", StringComparison.Ordinal))
                : username;
            _logger.Information("DebugPasswordChangeProvider.PerformPasswordChange: currentUsername=" + currentUsername);

            // Even in DEBUG, it is safe to make this call and check the password anyway
            if (PwnedPasswordsSearch.PwnedSearch.IsPwnedPassword(newPassword))
                return new ApiErrorItem(ApiErrorCode.PwnedPassword);

            return currentUsername switch
            {
                "error" => new ApiErrorItem(ApiErrorCode.Generic, "Error"),
                "changeNotPermitted" => new ApiErrorItem(ApiErrorCode.ChangeNotPermitted),
                "fieldMismatch" => new ApiErrorItem(ApiErrorCode.FieldMismatch),
                "fieldRequired" => new ApiErrorItem(ApiErrorCode.FieldRequired),
                "invalidCaptcha" => new ApiErrorItem(ApiErrorCode.InvalidCaptcha),
                "invalidCredentials" => new ApiErrorItem(ApiErrorCode.InvalidCredentials),
                "invalidDomain" => new ApiErrorItem(ApiErrorCode.InvalidDomain),
                "userNotFound" => new ApiErrorItem(ApiErrorCode.UserNotFound),
                "ldapProblem" => new ApiErrorItem(ApiErrorCode.LdapProblem),
                "pwnedPassword" => new ApiErrorItem(ApiErrorCode.PwnedPassword),
                _ => null
            };
        }

        public UserPrincipal GetUserPrincipal(string username, string pw)
        {
            throw new NotImplementedException();
        }

        public DirectoryEntry GetUserDirectoryEntry(string username, string pw)
        {
            throw new NotImplementedException();
        }

        public List<ApiResultAd> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public ApiResultAd? GetUserInfo(string username, string pw)
        {
            throw new NotImplementedException();
        }
    }
}
