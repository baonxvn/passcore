﻿namespace Zyborg.PassCore.PasswordProvider.LDAP
{
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using Microsoft.Extensions.Options;
    using Novell.Directory.Ldap;
    using Serilog;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using Unosquare.PassCore.Common;
    using System.Collections.Generic;
    using Hpl.HrmDatabase.ViewModels;
    using LdapRemoteCertificateValidationCallback =
        Novell.Directory.Ldap.RemoteCertificateValidationCallback;

    /// <summary>
    /// Represents a LDAP password change provider using Novell LDAP Connection.
    /// </summary>
    /// <seealso cref="IPasswordChangeProvider" />
    public class LdapPasswordChangeProvider : IPasswordChangeProvider
    {
        private readonly LdapPasswordChangeOptions _options;
        private readonly ILogger _logger;

        // First find user DN by username (SAM Account Name)
        private readonly LdapSearchConstraints _searchConstraints = new LdapSearchConstraints(
                0,
                0,
                LdapSearchConstraints.DerefNever,
                1000,
                true,
                1,
                null,
                10);

        private LdapRemoteCertificateValidationCallback _ldapRemoteCertValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapPasswordChangeProvider"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The _options.</param>
        public LdapPasswordChangeProvider(ILogger logger, IOptions<LdapPasswordChangeOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            Init();
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

        /// <inheritdoc />
        /// <remarks>
        /// Based on:
        ///    * https://www.cs.bham.ac.uk/~smp/resources/ad-passwds/
        ///    * https://support.microsoft.com/en-us/help/269190/how-to-change-a-windows-active-directory-and-lds-user-password-through
        ///    * https://ltb-project.org/documentation/self-service-password/latest/config_ldap#active_directory
        ///    * https://technet.microsoft.com/en-us/library/ff848710.aspx?f=255&amp;MSPPError=-2147217396
        ///
        /// Check the above links for more information.
        /// </remarks>
        public ApiErrorItem? PerformPasswordChange(string username, string currentPassword, string newPassword)
        {
            try
            {
                _logger.Information("Zyborg.PerformPasswordChange: username=" + username +
                            ". currentPassword=" + currentPassword +
                            ". newPassword=" + newPassword);

                var cleanUsername = CleaningUsername(username);
                _logger.Information("Zyborg.PerformPasswordChange: cleanUsername=" + cleanUsername);

                var searchFilter = _options.LdapSearchFilter.Replace("{Username}", cleanUsername);
                _logger.Information("Zyborg.PerformPasswordChange: searchFilter=" + searchFilter);

                _logger.Warning("LDAP query: {0}", searchFilter);

                using var ldap = BindToLdap();
                var search = ldap.Search(
                    _options.LdapSearchBase,
                    LdapConnection.ScopeSub,
                    searchFilter,
                    new[] { "distinguishedName" },
                    false,
                    _searchConstraints);

                // We cannot use search.Count here -- apparently it does not
                // wait for the results to return before resolving the count
                // but fortunately hasMore seems to block until final result
                if (!search.HasMore())
                {
                    _logger.Warning("Unable to find username: [{0}]", cleanUsername);

                    return new ApiErrorItem(
                        _options.HideUserNotFound ? ApiErrorCode.InvalidCredentials : ApiErrorCode.UserNotFound,
                        _options.HideUserNotFound ? "Invalid credentials" : "Username could not be located");
                }

                if (search.Count > 1)
                {
                    _logger.Warning("Found multiple with same username: [{0}] - Count {1}", cleanUsername, search.Count);

                    // Hopefully this should not ever happen if AD is preserving SAM Account Name
                    // uniqueness constraint, but just in case, handling this corner case
                    return new ApiErrorItem(ApiErrorCode.UserNotFound, "Multiple matching user entries resolved");
                }

                var userDN = search.Next().Dn;

                if (_options.LdapChangePasswordWithDelAdd)
                {
                    ChangePasswordDelAdd(currentPassword, newPassword, ldap, userDN);
                }
                else
                {
                    ChangePasswordReplace(newPassword, ldap, userDN);
                }

                if (_options.LdapStartTls)
                    ldap.StopTls();

                ldap.Disconnect();
            }
            catch (LdapException ex)
            {
                var item = ParseLdapException(ex);

                _logger.Warning(item.Message, ex);

                return item;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                var item = ex is ApiErrorException apiError
                    ? apiError.ToApiErrorItem()
                    : new ApiErrorItem(ApiErrorCode.InvalidCredentials, $"Failed to update password: {ex.Message}");

                _logger.Warning(item.Message, ex);

                return item;
            }

            // Everything seems to have worked:
            return null;
        }

        public int MeasureNewPasswordDistance(string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
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
            _logger.Information("START Novell.Directory.Ldap.LdapPasswordChangeProvider.IPasswordChangeProvider.GetAllUser");
            throw new NotImplementedException();
        }

        public ApiResultAd? GetUserInfo(string username, string pw)
        {
            _logger.Information("START Novell.Directory.Ldap.LdapPasswordChangeProvider.GetUserInfo");
            var result = new ApiResultAd();
            try
            {
                var cleanUsername = CleaningUsername(username);
                _logger.Information("Zyborg.PerformPasswordChange: cleanUsername=" + cleanUsername);

                var searchFilter = _options.LdapSearchFilter.Replace("{Username}", cleanUsername);
                _logger.Information("Zyborg.PerformPasswordChange: searchFilter=" + searchFilter);

                _logger.Warning("LDAP query: {0}", searchFilter);

                using var ldap = BindToLdap();
                var search = ldap.Search(
                    _options.LdapSearchBase,
                    LdapConnection.ScopeSub,
                    searchFilter,
                    new[] { "distinguishedName" },
                    false,
                    _searchConstraints);

                // We cannot use search.Count here -- apparently it does not
                // wait for the results to return before resolving the count
                // but fortunately hasMore seems to block until final result
                if (!search.HasMore())
                {
                    _logger.Warning("Unable to find username: [{0}]", cleanUsername);

                    //result.Errors = new ApiErrorItem(ApiErrorCode.InvalidCredentials, "Mật khẩu không đúng!");
                    result.Errors = new ApiErrorItem(_options.HideUserNotFound ? ApiErrorCode.InvalidCredentials : ApiErrorCode.UserNotFound,
                        _options.HideUserNotFound ? "Invalid credentials" : "Username could not be located");

                    return result;
                }

                if (search.Count > 1)
                {
                    _logger.Warning("Found multiple with same username: [{0}] - Count {1}", cleanUsername, search.Count);

                    // Hopefully this should not ever happen if AD is preserving SAM Account Name
                    // uniqueness constraint, but just in case, handling this corner case
                    result.Errors = new ApiErrorItem(ApiErrorCode.UserNotFound, "Multiple matching user entries resolved");
                    return result;
                }

                var userDN = search.Next().Dn;
                while (search.HasMore())
                {
                    LdapEntry nextEntry = null;
                    try
                    {
                        nextEntry = search.Next();
                    }
                    catch (LdapException e)
                    {
                        _logger.Error("Error: " + e.LdapErrorMessage);
                        //Console.WriteLine("Error: " + e.LdapErrorMessage);
                        // Exception is thrown, go for next entry
                        continue;
                    }
                    _logger.Warning("==>User: " + nextEntry.Dn);
                    //Console.WriteLine("\n" + nextEntry.Dn);
                    LdapAttributeSet attributeSet = nextEntry.GetAttributeSet();
                    System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
                    while (ienum.MoveNext())
                    {
                        LdapAttribute attribute = (LdapAttribute)ienum.Current;
                        string attributeName = attribute.Name;
                        string attributeVal = attribute.StringValue;
                        _logger.Warning(attributeName + " value:" + attributeVal);
                        //Console.WriteLine(attributeName + "value:" + attributeVal);
                    }
                }

                //LdapAttributeSet attributeSet = new LdapAttributeSet();
                //attributeSet.GetAttribute("");

                if (_options.LdapStartTls)
                    ldap.StopTls();

                ldap.Disconnect();
            }
            catch (LdapException ex)
            {
                result.Errors = ParseLdapException(ex);

                _logger.Warning(ex.Message);

                return result;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                result.Errors = ex is ApiErrorException apiError
                    ? apiError.ToApiErrorItem()
                    : new ApiErrorItem(ApiErrorCode.InvalidCredentials, $"Failed to update password: {ex.Message}");

                _logger.Warning(ex.Message);

                return result;
            }

            // Everything seems to have worked:
            return null;
        }

        private static void ChangePasswordReplace(string newPassword, ILdapConnection ldap, string userDN)
        {
            // If you don't have the rights to Add and/or Delete the Attribute, you might have the right to change the password-attribute.
            // In this case uncomment the next 2 lines and comment the region 'Change Password by Delete/Add'
            var attribute = new LdapAttribute("userPassword", newPassword);
            var ldapReplace = new LdapModification(LdapModification.Replace, attribute);
            ldap.Modify(userDN, new[] { ldapReplace }); // Change with Replace
        }

        private static void ChangePasswordDelAdd(string currentPassword, string newPassword, ILdapConnection ldap, string userDN)
        {
            var oldPassBytes = Encoding.Unicode.GetBytes($@"""{currentPassword}""");
            var newPassBytes = Encoding.Unicode.GetBytes($@"""{newPassword}""");

            var oldAttr = new LdapAttribute("unicodePwd", oldPassBytes);
            var newAttr = new LdapAttribute("unicodePwd", newPassBytes);

            var ldapDel = new LdapModification(LdapModification.Delete, oldAttr);
            var ldapAdd = new LdapModification(LdapModification.Add, newAttr);
            ldap.Modify(userDN, new[] { ldapDel, ldapAdd }); // Change with Delete/Add
        }

        private static ApiErrorItem ParseLdapException(LdapException ex)
        {
            // If the LDAP server returned an error, it will be formatted
            // similar to this:
            //    "0000052D: AtrErr: DSID-03191083, #1:\n\t0: 0000052D: DSID-03191083, problem 1005 (CONSTRAINT_ATT_TYPE), data 0, Att 9005a (unicodePwd)\n\0"
            //
            // The leading number before the ':' is the Win32 API Error Code in HEX
            if (ex.LdapErrorMessage == null)
            {
                return new ApiErrorItem(ApiErrorCode.LdapProblem, "Unexpected null exception");
            }

            var m = Regex.Match(ex.LdapErrorMessage, "([0-9a-fA-F]+):");

            if (!m.Success)
            {
                return new ApiErrorItem(ApiErrorCode.LdapProblem, $"Unexpected error: {ex.LdapErrorMessage}");
            }

            var errCodeString = m.Groups[1].Value;
            var errCode = int.Parse(errCodeString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var err = Win32ErrorCode.ByCode(errCode);

            return err == null
                ? new ApiErrorItem(ApiErrorCode.LdapProblem, $"Unexpected Win32 API error; error code: {errCodeString}")
                : new ApiErrorItem(ApiErrorCode.InvalidCredentials,
                    $"Resolved Win32 API Error: code={err.Code} name={err.CodeName} desc={err.Description}");
        }

        private string CleaningUsername(string username)
        {
            var cleanUsername = username;
            var index = cleanUsername.IndexOf("@", StringComparison.Ordinal);
            if (index >= 0)
                cleanUsername = cleanUsername.Substring(0, index);

            // Must sanitize the username to eliminate the possibility of injection attacks:
            //    * https://docs.microsoft.com/en-us/windows/desktop/adschema/a-samaccountname
            //    * https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-2000-server/bb726984(v=technet.10)
            var invalidChars = "\"/\\[]:;|=,+*?<>\r\n\t".ToCharArray();

            if (cleanUsername.IndexOfAny(invalidChars) >= 0)
            {
                throw new ApiErrorException("Username contains one or more invalid characters", ApiErrorCode.InvalidCredentials);
            }

            // LDAP filters require escaping of some special chars:
            //    * http://www.ldapexplorer.com/en/manual/109010000-ldap-filter-syntax.htm
            var escape = "()&|=><!*/\\".ToCharArray();
            var escapeIndex = cleanUsername.IndexOfAny(escape);

            if (escapeIndex < 0)
                return cleanUsername;

            var buff = new StringBuilder();
            var maxLen = cleanUsername.Length;
            var copyFrom = 0;

            while (escapeIndex >= 0)
            {
                buff.Append(cleanUsername.Substring(copyFrom, escapeIndex));
                buff.Append(string.Format("\\{0:X}", (int)cleanUsername[escapeIndex]));
                copyFrom = escapeIndex + 1;
                escapeIndex = cleanUsername.IndexOfAny(escape, copyFrom);
            }

            if (copyFrom < maxLen)
                buff.Append(cleanUsername.Substring(copyFrom));
            cleanUsername = buff.ToString();
            _logger.Warning("Had to clean username: [{0}] => [{1}]", username, cleanUsername);

            return cleanUsername;
        }

        private void Init()
        {
            // Validate required options
            if (_options.LdapIgnoreTlsErrors || _options.LdapIgnoreTlsValidation)
                _ldapRemoteCertValidator = CustomServerCertValidation;

            if (_options.LdapHostnames?.Length < 1)
            {
                throw new ArgumentException("Options must specify at least one LDAP hostname",
                    nameof(_options.LdapHostnames));
            }

            if (string.IsNullOrEmpty(_options.LdapUsername))
            {
                throw new ArgumentException("Options missing or invalid LDAP bind distinguished name (DN)",
                    nameof(_options.LdapUsername));
            }

            if (string.IsNullOrEmpty(_options.LdapPassword))
            {
                throw new ArgumentException("Options missing or invalid LDAP bind password",
                    nameof(_options.LdapPassword));
            }

            if (string.IsNullOrEmpty(_options.LdapSearchBase))
            {
                throw new ArgumentException("Options must specify LDAP search base",
                    nameof(_options.LdapSearchBase));
            }

            if (string.IsNullOrWhiteSpace(_options.LdapSearchFilter))
            {
                throw new ArgumentException(
                    $"No {nameof(_options.LdapSearchFilter)} is set. Fill attribute {nameof(_options.LdapSearchFilter)} in file appsettings.json",
                    nameof(_options.LdapSearchFilter));
            }

            if (!_options.LdapSearchFilter.Contains("{Username}"))
            {
                throw new ArgumentException(
                    $"The {nameof(_options.LdapSearchFilter)} should include {{Username}} value in the template string",
                    nameof(_options.LdapSearchFilter));
            }

            // All other configuration is optional, but some may warrant attention
            if (!_options.HideUserNotFound)
                _logger.Warning($"Option [{nameof(_options.HideUserNotFound)}] is DISABLED; the presence or absence of usernames can be harvested");

            if (_options.LdapIgnoreTlsErrors)
                _logger.Warning($"Option [{nameof(_options.LdapIgnoreTlsErrors)}] is ENABLED; invalid certificates will be allowed");
            else if (_options.LdapIgnoreTlsValidation)
                _logger.Warning($"Option [{nameof(_options.LdapIgnoreTlsValidation)}] is ENABLED; untrusted certificate roots will be allowed");

            if (_options.LdapPort == LdapConnection.DefaultSslPort && !_options.LdapSecureSocketLayer)
                _logger.Warning($"Option [{nameof(_options.LdapSecureSocketLayer)}] is DISABLED in combination with standard SSL port [{_options.LdapPort}]");

            if (_options.LdapPort != LdapConnection.DefaultSslPort && !_options.LdapStartTls)
                _logger.Warning($"Option [{nameof(_options.LdapStartTls)}] is DISABLED in combination with non-standard TLS port [{_options.LdapPort}]");
        }

        private LdapConnection BindToLdap()
        {
            var ldap = new LdapConnection();
            if (_ldapRemoteCertValidator != null)
                ldap.UserDefinedServerCertValidationDelegate += _ldapRemoteCertValidator;

            ldap.SecureSocketLayer = _options.LdapSecureSocketLayer;

            string? bindHostname = null;

            foreach (var h in _options.LdapHostnames)
            {
                try
                {
                    ldap.Connect(h, _options.LdapPort);
                    bindHostname = h;
                    break;
                }
                catch (LdapException ex)
                {
                    _logger.Error($"Failed to connect to host [{h}]", ex);
                }
            }

            if (string.IsNullOrEmpty(bindHostname))
            {
                throw new ApiErrorException("Failed to connect to any configured hostname", ApiErrorCode.InvalidCredentials);
            }

            if (_options.LdapStartTls)
            {
                try
                {
                    ldap.StartTls();
                }
                catch (LdapException e)
                {
                    _logger.Error("Lỗi ldap.StartTls: " + e);
                    _logger.Error("Lỗi ldap.StartTls: " + e.LdapErrorMessage);
                }
            }

            try
            {
                ldap.Bind(_options.LdapUsername, _options.LdapPassword);
            }
            catch (LdapException e)
            {
                _logger.Error("Lỗi ldap.Bind: " + e);
                _logger.Error("Lỗi ldap.Bind: " + e.LdapErrorMessage);
            }

            return ldap;
        }

        /// <summary>
        /// Custom server certificate validation logic that handles our special
        /// cases based on configuration.  This implements the logic of either
        /// ignoring just untrusted root errors or ignoring all TLS errors.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslPolicyErrors">The SSL policy errors.</param>
        /// <returns><c>true</c> if the certificate validation was successful.</returns>
        private bool CustomServerCertValidation(
                    object sender,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors) =>
            _options.LdapIgnoreTlsErrors || sslPolicyErrors == SslPolicyErrors.None || chain.ChainStatus
                .Any(x => x.Status switch
                {
                    X509ChainStatusFlags.UntrustedRoot when _options.LdapIgnoreTlsValidation => true,
                    _ => x.Status == X509ChainStatusFlags.NoError
                });
    }
}
