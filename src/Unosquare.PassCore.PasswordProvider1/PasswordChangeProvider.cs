namespace Unosquare.PassCore.PasswordProvider
{
    using Common;
    using Microsoft.Extensions.Options;
    using Serilog;
    using Serilog.Sinks.EventLog;
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using System.DirectoryServices.ActiveDirectory;
    using System.Linq;
    using Swan.Formatters;
    using Swan;

    /// <inheritdoc />
    /// <summary>
    /// Default Change Password Provider using 'System.DirectoryServices' from Microsoft.
    /// </summary>
    /// <seealso cref="IPasswordChangeProvider" />
    public partial class PasswordChangeProvider : IPasswordChangeProvider
    {
        private readonly PasswordChangeOptions _options;
        private readonly ILogger _logger;
        private IdentityType _idType = IdentityType.UserPrincipalName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordChangeProvider"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        public PasswordChangeProvider(
            ILogger logger,
            IOptions<PasswordChangeOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            SetIdType();
        }

        public ApiErrorItem? GetAllUser()
        {
            int i = 0;
            using (var searcher = new PrincipalSearcher(new UserPrincipal(new PrincipalContext(ContextType.Domain, Environment.UserDomainName))))
            {
                List<UserPrincipal> users = searcher.FindAll().Select(u => (UserPrincipal)u).ToList();
                i = users.Count;
                //foreach (var u in users)
                //{
                //    DirectoryEntry d = (DirectoryEntry)u.GetUnderlyingObject();
                //    Console.WriteLine(d.Properties["GivenName"]?.Value?.ToString() + d.Properties["sn"]?.Value?.ToString());
                //}
            }

            return new ApiErrorItem(ApiErrorCode.Generic, "Khong co user nao"); ;
        }

        public UserPrincipal GetUserPrincipal(string username, string pw)
        {
            var fixedUsername = FixUsernameWithDomain(username);
            //using var principalContext = AcquirePrincipalContext();
            using var principalContext = AcquirePrincipalContext(username, pw);
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);

            return userPrincipal;
        }

        public DirectoryEntry GetUserDirectoryEntry(string username, string pw)
        {
            var fixedUsername = FixUsernameWithDomain(username);
            using var principalContext = AcquirePrincipalContext();
            //using var principalContext = AcquirePrincipalContext(username, pw);
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);
            if (userPrincipal != null)
            {
                var directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;

                return directoryEntry;
            }

            return null;
        }

        //public ApiResultAd? GetUserInfo(string username, string pw)
        public ApiResultAd? GetUserInfo(string username, string pw)
        {
            _logger.Information("PasswordChangeProvider.GetUserInfo");
            var result = new ApiResultAd();
            result.UserInfo = null;

            var fixedUsername = FixUsernameWithDomain(username);
            using var principalContext = AcquirePrincipalContext();
            //using var principalContext = AcquirePrincipalContext(username, pw);//Không sử dụng user trong setting
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);

            // Check if the user principal exists
            if (userPrincipal == null)
            {
                _logger.Warning($"The User principal ({fixedUsername}) doesn't exist");
                result.Errors = new ApiErrorItem(ApiErrorCode.UserNotFound, "User khong ton tai!");

                return result;
            }

            var item = ValidateGroups(userPrincipal);

            if (item != null)
            {
                result.Errors = item;
                return result;
            }

            // Use always UPN for password check.
            if (!ValidateUserCredentials(userPrincipal.UserPrincipalName, pw, principalContext))
            {
                _logger.Warning("The User principal password is not valid");

                result.Errors = new ApiErrorItem(ApiErrorCode.InvalidCredentials, "Mật khẩu không đúng!");
                return result;
            }

            var userInfo = new UserInfoAd
            {
                isLocked = userPrincipal.IsAccountLockedOut(),
                displayName = userPrincipal.DisplayName,
                userPrincipalName = userPrincipal.UserPrincipalName,
                sAMAccountName = userPrincipal.SamAccountName,
                givenName = userPrincipal.GivenName,
                name = userPrincipal.Name,
                sn = userPrincipal.Surname,
                description = userPrincipal.Description,
                mail = userPrincipal.EmailAddress,
                telephoneNumber = userPrincipal.VoiceTelephoneNumber,
                otherTelephone = "",
                physicalDeliveryOfficeName = "",
                initials = "",
                wWWHomePage = "",
                url = "",
                CN = "",
                homePhone = "",
                mobile = ""
            };

            if (userPrincipal.GetUnderlyingObject() is DirectoryEntry directoryEntry)
            {
                //department: Chi nhánh/Phòng ban
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Department))
                {
                    userInfo.department = directoryEntry.Properties[UserPropertiesAd.Department].Value.ToString();
                }
                //title = Chức danh
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Title))
                {
                    userInfo.title = directoryEntry.Properties[UserPropertiesAd.Title].Value.ToString();
                }
                //employeeID mã nhân viên
                if (directoryEntry.Properties.Contains(UserPropertiesAd.EmployeeId))
                {
                    userInfo.employeeID = directoryEntry.Properties[UserPropertiesAd.EmployeeId].Value.ToString();
                }
                //mobile=Điện thoại
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Mobile))
                {
                    userInfo.mobile = directoryEntry.Properties[UserPropertiesAd.Mobile].Value.ToString();
                }

                if (directoryEntry.Properties.Contains(UserPropertiesAd.Homephone))
                {
                    userInfo.homePhone = directoryEntry.Properties[UserPropertiesAd.Homephone].Value.ToString();
                }

                //prop.Value = -1;
                //directoryEntry.Properties[UserPropertiesAd.Department].Value = "Day la phong ban moi";
                //directoryEntry.CommitChanges();
            }

            result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
            result.UserInfo = userInfo;

            return result;
        }

        /// <summary>
        /// Cập nhật một số thông tin user trên AD
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public ApiResultAd? UpdateUserInfo(string username)
        {
            _logger.Information("PasswordChangeProvider.GetUserInfo");
            var result = new ApiResultAd { UserInfo = null };

            var fixedUsername = FixUsernameWithDomain(username);
            using var principalContext = AcquirePrincipalContext();
            //using var principalContext = AcquirePrincipalContext(username, pw);//Không sử dụng user trong setting
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);

            // Check if the user principal exists
            if (userPrincipal == null)
            {
                _logger.Warning($"The User principal ({fixedUsername}) doesn't exist");
                result.Errors = new ApiErrorItem(ApiErrorCode.UserNotFound, "User khong ton tai!");

                return result;
            }

            var item = ValidateGroups(userPrincipal);

            if (item != null)
            {
                result.Errors = item;
                return result;
            }

            var userInfo = new UserInfoAd
            {
                isLocked = userPrincipal.IsAccountLockedOut(),
                displayName = userPrincipal.DisplayName,
                userPrincipalName = userPrincipal.UserPrincipalName,
                sAMAccountName = userPrincipal.SamAccountName,
                givenName = userPrincipal.GivenName,
                name = userPrincipal.Name,
                sn = userPrincipal.Surname,
                description = userPrincipal.Description,
                mail = userPrincipal.EmailAddress,
                telephoneNumber = userPrincipal.VoiceTelephoneNumber,
                //otherTelephone = "",
                //physicalDeliveryOfficeName = "",
                //initials = "",
                //wWWHomePage = "",
                //url = "",
                //CN = "",
                //homePhone = "",
                //mobile = ""
            };

            if (userPrincipal.GetUnderlyingObject() is DirectoryEntry directoryEntry)
            {
                //department: Chi nhánh/Phòng ban
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Department))
                {
                    userInfo.department = directoryEntry.Properties[UserPropertiesAd.Department].Value.ToString();
                }
                //title = Chức danh
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Title))
                {
                    userInfo.title = directoryEntry.Properties[UserPropertiesAd.Title].Value.ToString();
                }
                //employeeID mã nhân viên
                if (directoryEntry.Properties.Contains(UserPropertiesAd.EmployeeId))
                {
                    userInfo.employeeID = directoryEntry.Properties[UserPropertiesAd.EmployeeId].Value.ToString();
                }
                //mobile=Điện thoại
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Mobile))
                {
                    userInfo.mobile = directoryEntry.Properties[UserPropertiesAd.Mobile].Value.ToString();
                }

                if (directoryEntry.Properties.Contains(UserPropertiesAd.Homephone))
                {
                    userInfo.homePhone = directoryEntry.Properties[UserPropertiesAd.Homephone].Value.ToString();
                }

                //prop.Value = -1;
                //directoryEntry.Properties[UserPropertiesAd.Department].Value = "Day la phong ban moi";
                //directoryEntry.CommitChanges();
            }

            result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
            result.UserInfo = userInfo;

            return result;
        }

        /// <inheritdoc />
        public ApiErrorItem? PerformPasswordChange(string username, string currentPassword, string newPassword)
        {
            try
            {
                _logger.Information("PasswordChangeProvider.PerformPasswordChange: username=" + username +
                    ". currentPassword=" + currentPassword +
                    ". newPassword=" + newPassword);

                var fixedUsername = FixUsernameWithDomain(username);
                _logger.Information("PasswordChangeProvider.PerformPasswordChange: fixedUsername=" + fixedUsername);

                using var principalContext = AcquirePrincipalContext();
                var userPrincipal = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);

                // Check if the user principal exists
                if (userPrincipal == null)
                {
                    _logger.Warning($"The User principal ({fixedUsername}) doesn't exist");

                    return new ApiErrorItem(ApiErrorCode.UserNotFound, "Khong ton tai user");
                }

                //BAONX
                //var minPwdLength = AcquireDomainPasswordLength();

                //if (newPassword.Length < minPwdLength)
                //{
                //    _logger.Error("Failed due to password complex policies: New password length is shorter than AD minimum password length");

                //    return new ApiErrorItem(ApiErrorCode.ComplexPassword);
                //}

                //// Check if the newPassword is Pwned
                //if (PwnedPasswordsSearch.PwnedSearch.IsPwnedPassword(newPassword))
                //{
                //    _logger.Error("Failed due to pwned password: New password is publicly known and can be used in dictionary attacks");

                //    return new ApiErrorItem(ApiErrorCode.PwnedPassword);
                //}

                _logger.Information($"PerformPasswordChange for user {fixedUsername}");

                var item = ValidateGroups(userPrincipal);

                if (item != null)
                    return item;

                // Check if password change is allowed
                if (userPrincipal.UserCannotChangePassword)
                {
                    _logger.Warning("The User principal cannot change the password");

                    return new ApiErrorItem(ApiErrorCode.ChangeNotPermitted);
                }

                // Check if password expired or must be changed
                if (_options.UpdateLastPassword && userPrincipal.LastPasswordSet == null)
                {
                    SetLastPassword(userPrincipal);
                }

                // Use always UPN for password check.
                if (!ValidateUserCredentials(userPrincipal.UserPrincipalName, currentPassword, principalContext))
                {
                    _logger.Warning("The User principal password is not valid");

                    return new ApiErrorItem(ApiErrorCode.InvalidCredentials);
                }

                // Change the password via 2 different methods. Try SetPassword if ChangePassword fails.
                ChangePassword(currentPassword, newPassword, userPrincipal);

                userPrincipal.Save();
                _logger.Debug("The User principal password updated with setPassword");
            }
            //BAONX
            //catch (PasswordException passwordEx)
            //{
            //    var item = new ApiErrorItem(ApiErrorCode.ComplexPassword, passwordEx.Message);

            //    _logger.Warning(item.Message, passwordEx);

            //    return item;
            //}
            catch (Exception ex)
            {
                var item = ex is ApiErrorException apiError
                    ? apiError.ToApiErrorItem()
                    : new ApiErrorItem(ApiErrorCode.Generic, ex.InnerException?.Message ?? ex.Message);

                _logger.Warning(item.Message, ex);

                return item;
            }

            return null;
        }

        private bool ValidateUserCredentials(
            string upn,
            string currentPassword,
            PrincipalContext principalContext)
        {
            if (principalContext.ValidateCredentials(upn, currentPassword))
                return true;

            if (LogonUser(upn, string.Empty, currentPassword, LogonTypes.Network, LogonProviders.Default, out _))
                return true;

            var errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

            _logger.Debug($"ValidateUserCredentials GetLastWin32Error {errorCode}");

            // Both of these means that the password CAN change and that we got the correct password
            return errorCode == ErrorPasswordMustChange || errorCode == ErrorPasswordExpired;
        }

        private string FixUsernameWithDomain(string username)
        {
            if (_idType != IdentityType.UserPrincipalName) return username;

            // Check for default domain: if none given, ensure EFLD can be used as an override.
            var parts = username.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            var domain = parts.Length > 1 ? parts[1] : _options.DefaultDomain;

            return string.IsNullOrWhiteSpace(domain) || parts.Length > 1 ? username : $"{username}@{domain}";
        }

        private ApiErrorItem? ValidateGroups(UserPrincipal userPrincipal)
        {
            try
            {
                PrincipalSearchResult<Principal> groups;

                try
                {
                    groups = userPrincipal.GetGroups();
                }
                catch (Exception exception)
                {
                    //_logger.Error(new EventId(887), exception, nameof(ValidateGroups));
                    string eventId = new Microsoft.Extensions.Logging.EventId(887).ToString();
                    _logger.Error(eventId, exception, nameof(ValidateGroups));

                    groups = userPrincipal.GetAuthorizationGroups();
                }

                if (groups.Any(x => _options.RestrictedADGroups.Contains(x.Name)))
                {
                    return new ApiErrorItem(ApiErrorCode.ChangeNotPermitted,
                        "The User principal is listed as restricted");
                }

                //BAONX
                if (_options.AllowedADGroups?.Any() != true) return null;
                //END BAONX

                var valueReturn = groups?.Any(x => _options.AllowedADGroups?.Contains(x.Name) == true) == true
                    ? null
                    : new ApiErrorItem(ApiErrorCode.ChangeNotPermitted, "The User principal is not listed as allowed");

                return valueReturn;
            }
            catch (Exception exception)
            {
                //_logger.Error(new EventId(888), exception, nameof(ValidateGroups));
                string eventId = new Microsoft.Extensions.Logging.EventId(888).ToString();
                _logger.Error(eventId, exception, nameof(ValidateGroups));
            }

            return null;
        }

        private void SetLastPassword(Principal userPrincipal)
        {
            var directoryEntry = (DirectoryEntry)userPrincipal.GetUnderlyingObject();
            var prop = directoryEntry.Properties["pwdLastSet"];

            if (prop == null)
            {
                _logger.Warning("The User principal password have no last password, but the property is missing");
                return;
            }

            try
            {
                prop.Value = -1;
                directoryEntry.CommitChanges();
                _logger.Warning("The User principal last password was updated");
            }
            catch (Exception ex)
            {
                throw new ApiErrorException($"Failed to update password: {ex.Message}",
                    ApiErrorCode.ChangeNotPermitted);
            }
        }

        private void ChangePassword(string currentPassword, string newPassword, AuthenticablePrincipal userPrincipal)
        {
            try
            {
                // Try by regular ChangePassword method
                _logger.Warning("Gọi method userPrincipal.ChangePassword()");
                userPrincipal.ChangePassword(currentPassword, newPassword);
            }
            catch (Exception e)
            {
                _logger.Debug("Lỗi khi call userPrincipal.ChangePassword: " + e.Message);
                if (_options.UseAutomaticContext)
                {
                    _logger.Warning("The User principal password cannot be changed and setPassword won't be called");

                    throw;
                }

                // If the previous attempt failed, use the SetPassword method.
                userPrincipal.SetPassword(newPassword);

                _logger.Debug("The User principal password updated with setPassword");
            }
        }

        /// <summary>
        /// Use the values from appsettings.IdTypeForUser as fault-tolerant as possible.
        /// </summary>
        private void SetIdType()
        {
            _idType = _options.IdTypeForUser?.Trim().ToLower() switch
            {
                "distinguishedname" => IdentityType.DistinguishedName,
                "distinguished name" => IdentityType.DistinguishedName,
                "dn" => IdentityType.DistinguishedName,
                "globally unique identifier" => IdentityType.Guid,
                "globallyuniqueidentifier" => IdentityType.Guid,
                "guid" => IdentityType.Guid,
                "name" => IdentityType.Name,
                "nm" => IdentityType.Name,
                "samaccountname" => IdentityType.SamAccountName,
                "accountname" => IdentityType.SamAccountName,
                "sam account" => IdentityType.SamAccountName,
                "sam account name" => IdentityType.SamAccountName,
                "sam" => IdentityType.SamAccountName,
                "securityidentifier" => IdentityType.Sid,
                "securityid" => IdentityType.Sid,
                "secid" => IdentityType.Sid,
                "security identifier" => IdentityType.Sid,
                "sid" => IdentityType.Sid,
                _ => IdentityType.UserPrincipalName
            };
        }

        /// <summary>
        /// Gọi hàm này một trong 2 trường hợp sau
        /// UseAutomaticContext=true:
        /// + Code phải đặt trên server AD và không cần điền thông tin Admin Quản trị AD
        /// UseAutomaticContext=false:
        /// + Code đặt trên AD hoặc ngoài AD đều được và bắt buộc phải điền thông tin Admin quản trị domain
        /// </summary>
        /// <returns></returns>
        private PrincipalContext AcquirePrincipalContext()
        {
            //_logger.Warning(_options.ToJson());

            if (_options.UseAutomaticContext)
            {
                _logger.Warning("Using AutomaticContext");
                return new PrincipalContext(ContextType.Domain);
            }

            var domain = $"{_options.LdapHostnames.First()}:{_options.LdapPort}";
            _logger.Warning($"Not using AutomaticContext  {domain}");

            return new PrincipalContext(
                ContextType.Domain,
                domain,
                _options.LdapUsername,
                _options.LdapPassword);
        }

        /// <summary>
        /// Gọi hàm này trong trường hợp (AutomaticContext=TRUE or FALSE không quan trọng)
        /// Khi source code không đặt trên server AD và không muốn điền thông tin(username&pass) của Admin quản trị domain
        /// Reset pass dựa vào Authorization Username & Password của User truyền vào.
        /// Trong file appsettings.json cần setting 2 tham số LdapHostnames:LdapPort.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        private PrincipalContext AcquirePrincipalContext(string username, string pw)
        {
            _logger.Warning("PasswordChangeProvider.AcquirePrincipalContext: " + _options.ToJson());

            var domain = $"{_options.LdapHostnames.First()}:{_options.LdapPort}";
            //_logger.Warning($"Not using AutomaticContext  {domain}");

            return new PrincipalContext(
                ContextType.Domain,
                domain,
                username,
                pw);
        }

        private int AcquireDomainPasswordLength()
        {
            DirectoryEntry entry;
            if (_options.UseAutomaticContext)
            {
                entry = Domain.GetCurrentDomain().GetDirectoryEntry();
            }
            else
            {
                entry = new DirectoryEntry(
                    $"{_options.LdapHostnames.First()}:{_options.LdapPort}",
                    _options.LdapUsername,
                    _options.LdapPassword
                    );
            }
            return (int)entry.Properties["minPwdLength"].Value;
        }
    }
}
