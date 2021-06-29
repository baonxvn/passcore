

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
    using Newtonsoft.Json;
    using Hpl.HrmDatabase.ViewModels;

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

        /// <summary>
        /// Get all user của AD
        /// </summary>
        /// <returns></returns>
        public List<ApiResultAd> GetAllUsers()
        {
            var listUser = new List<ApiResultAd>();
            var result = new ApiResultAd();
            int i = 0;

            //using (var searcher = new PrincipalSearcher(new UserPrincipal(new PrincipalContext(ContextType.Domain, Environment.UserDomainName))))

            var principalContext = AcquirePrincipalContext();
            var userPrincipal = new UserPrincipal(principalContext);
            using var searcher = new PrincipalSearcher(userPrincipal);

            List<UserPrincipal> users = searcher.FindAll().Select(u => (UserPrincipal)u).ToList();

            if (users.Any())
            {
                foreach (var u in users)
                {
                    DirectoryEntry dirEntry = (DirectoryEntry)u.GetUnderlyingObject();
                    result.UserInfo = ConvertUserProfiles(dirEntry);
                    listUser.Add(result);
                    u.Dispose();
                }
            }
            else
            {
                result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Không có user nào.");
            }

            principalContext.Dispose();
            userPrincipal.Dispose();
            searcher.Dispose();

            return listUser;
        }

        public UserPrincipal GetUserPrincipal(string username, string pw)
        {
            var fixedUsername = FixUsernameWithDomain(username);
            //using var principalContext = AcquirePrincipalContext();
            using var principalContext = AcquirePrincipalContext(username, pw);
            return UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);
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

            //Không cần check nhóm
            //var item = ValidateGroups(userPrincipal);
            //if (item != null)
            //{
            //    result.Errors = item;
            //    return result;
            //}

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
                name = userPrincipal.Name,
                givenName = userPrincipal.GivenName,//Họ LastName
                sn = userPrincipal.Surname,//Tên FirstName
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
                userInfo = ConvertUserProfiles(directoryEntry);
                directoryEntry.Dispose();
            }

            result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
            result.UserInfo = userInfo;

            principalContext.Dispose();
            userPrincipal.Dispose();

            return result;
        }

        public string GetUserNewUserFormAd(string username)
        {
            _logger.Information("PasswordChangeProvider.GetUserNewUserFormAd");

            string newUser = username;
            var principalContext = AcquirePrincipalContext();

            //Kiểm tra user đã tồn tại chưa
            bool check = true;
            int i = 0;
            while (check)
            {
                string fixedUsername = FixUsernameWithDomain(newUser);
                var up = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);
                if (up != null)
                {
                    i++;
                    newUser = username + i;
                    up.Dispose();
                }
                else
                {
                    check = false;
                }
            }

            return newUser;
        }

        /// <summary>
        /// Cập nhật một số thông tin user trên AD
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<ApiResultAd> UpdateUserInfo(List<NhanVienViewModel> listNvs)
        {
            _logger.Information("START PasswordChangeProvider.UpdateUserInfo");
            List<ApiResultAd> listResult = new List<ApiResultAd>();
            var result = new ApiResultAd();

            using var principalContext = AcquirePrincipalContext();
            if (principalContext == null)
            {
                result.Errors = new ApiErrorItem(ApiErrorCode.InvalidCredentials, "Không kết nối được đến server AD.");
                listResult.Add(result);
                return listResult;

            }
            //using var principalContext = AcquirePrincipalContext(username, pw);//Không sử dụng user trong setting
            foreach (var model in listNvs)
            {
                var fixedUsername = FixUsernameWithDomain(model.TenDangNhap);
                var userPrincipal = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);

                // Check if the user principal exists
                if (userPrincipal == null)
                {
                    _logger.Warning($"The User principal ({fixedUsername}) doesn't exist");
                    result.Errors = new ApiErrorItem(ApiErrorCode.UserNotFound, "User " + model.TenDangNhap + " không tồn tại trên AD.");

                    listResult.Add(result);
                    continue;
                }

                //Không cần check Groups
                //var item = ValidateGroups(userPrincipal);
                //if (item != null)
                //{
                //    result.Errors = item;
                //    listResult.Add(result);
                //    break;
                //}

                //Lấy thông tin của User
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

                var ten = CommonHelper.ConvertToUnSign(model.Ten);
                var ho = CommonHelper.ConvertToUnSign(model.Ho);

                bool checkForUpdate = false;
                if (userPrincipal.GetUnderlyingObject() is DirectoryEntry adEntry)
                {
                    //var xx = CommonHelper.ConvertToUnSign(model.xx);
                    //userInfo.sn = xx;
                    //if (directoryEntry.Properties.Contains(UserPropertiesAd.xx))
                    //{
                    //    if (!model.xx.Equals(directoryEntry.Properties[UserPropertiesAd.xx].Value.ToString()))
                    //    {
                    //        directoryEntry.Properties[UserPropertiesAd.xx].Value = model.xx;
                    //        checkForUpdate = true;
                    //    }
                    //}
                    //else
                    //{
                    //    directoryEntry.Properties[UserPropertiesAd.xx].Value = model.xx;
                    //    checkForUpdate = true;
                    //}

                }

                checkForUpdate = false;
                if (userPrincipal.GetUnderlyingObject() is DirectoryEntry directoryEntry)
                {
                    //Update lại tên của Nhân sự
                    //Fix CN=Ho va Ten
                    try
                    {
                        if (directoryEntry.Properties.Contains(UserPropertiesAd.ContainerName))
                        {
                            if (!model.TenDangNhap.Equals(directoryEntry.Properties[UserPropertiesAd.LoginName].Value.ToString()))
                            {
                                directoryEntry.Rename("CN=" + ho + " " + ten);
                            }
                        }
                        else
                        {
                            directoryEntry.Rename("CN=" + ho + " " + ten);
                        }

                        result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
                    }
                    catch (Exception e)
                    {
                        result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Update dữ liệu " + model.TenDangNhap + " vào AD không thành công. Error: " + e.Message);
                        listResult.Add(result);
                        continue;
                    }

                    //Fix HỌ: sn=LastName=Ho; 
                    userInfo.sn = ho;
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.LastName))
                    {
                        if (!ho.Equals(directoryEntry.Properties[UserPropertiesAd.LastName].Value.ToString()))
                        {
                            directoryEntry.Properties[UserPropertiesAd.LastName].Value = ho;
                            checkForUpdate = true;
                        }
                    }
                    else
                    {
                        directoryEntry.Properties[UserPropertiesAd.LastName].Value = ho;
                        checkForUpdate = true;
                    }

                    //Tên: givenName = First name = Ten
                    userInfo.givenName = ten;
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.FirstName))
                    {
                        if (!ten.Equals(directoryEntry.Properties[UserPropertiesAd.FirstName].Value.ToString()))
                        {
                            directoryEntry.Properties[UserPropertiesAd.FirstName].Value = ten;
                            checkForUpdate = true;
                        }
                    }
                    else
                    {
                        directoryEntry.Properties[UserPropertiesAd.FirstName].Value = ten;
                        checkForUpdate = true;
                    }


                    //displayName=(Ho va ten)
                    var displayName = ho + " " + ten;
                    userInfo.displayName = displayName;
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.DisplayName))
                    {
                        if (!displayName.Equals(directoryEntry.Properties[UserPropertiesAd.DisplayName].Value.ToString()))
                        {
                            directoryEntry.Properties[UserPropertiesAd.DisplayName].Value = displayName;
                            checkForUpdate = true;
                        }
                    }
                    else
                    {
                        directoryEntry.Properties[UserPropertiesAd.DisplayName].Value = displayName;
                        checkForUpdate = true;
                    }

                    //department: Chi nhánh/Phòng ban
                    userInfo.department = model.TenPhongBan;
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.Department))
                    {
                        if (!model.TenPhongBan.Equals(directoryEntry.Properties[UserPropertiesAd.Department].Value.ToString()))
                        {
                            directoryEntry.Properties[UserPropertiesAd.Department].Value = model.TenPhongBan;
                            checkForUpdate = true;
                        }
                    }
                    else
                    {
                        directoryEntry.Properties[UserPropertiesAd.Department].Value = model.TenPhongBan;
                        checkForUpdate = true;
                    }

                    //title = Chức danh
                    userInfo.title = model.TenChucDanh;
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.Title))
                    {
                        if (!model.TenChucDanh.Equals(directoryEntry.Properties[UserPropertiesAd.Title].Value.ToString()))
                        {
                            directoryEntry.Properties[UserPropertiesAd.Title].Value = model.TenChucDanh;
                            checkForUpdate = true;
                        }
                    }
                    else
                    {
                        directoryEntry.Properties[UserPropertiesAd.Title].Value = model.TenChucDanh;
                        checkForUpdate = true;
                    }

                    //employeeID mã nhân viên
                    userInfo.employeeID = model.MaNhanVien;
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.EmployeeId))
                    {
                        if (!model.MaNhanVien.Equals(directoryEntry.Properties[UserPropertiesAd.EmployeeId].Value.ToString()))
                        {
                            directoryEntry.Properties[UserPropertiesAd.EmployeeId].Value = model.MaNhanVien;
                            checkForUpdate = true;
                        }
                    }
                    else
                    {
                        directoryEntry.Properties[UserPropertiesAd.EmployeeId].Value = model.MaNhanVien;
                        checkForUpdate = true;
                    }

                    //telephoneNumber=Điện thoại
                    try
                    {
                        string dt = "+84" + int.Parse(model.DienThoai);
                        userInfo.telephoneNumber = dt;
                        if (directoryEntry.Properties.Contains(UserPropertiesAd.TelePhoneNumber))
                        {
                            if (!dt.Equals(directoryEntry.Properties[UserPropertiesAd.TelePhoneNumber].Value.ToString()))
                            {
                                directoryEntry.Properties[UserPropertiesAd.TelePhoneNumber].Value = dt;
                                checkForUpdate = true;
                            }
                        }
                        else
                        {
                            directoryEntry.Properties[UserPropertiesAd.TelePhoneNumber].Value = dt;
                            checkForUpdate = true;
                        }
                    }
                    catch (Exception)
                    {
                        result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Số điện thoại của " + model.TenDangNhap + " không đúng.");
                    }

                    var email = CommonHelper.IsValidEmail(model.Email);
                    userInfo.mail = email;
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.EmailAddress))
                    {
                        if (!email.Equals(directoryEntry.Properties[UserPropertiesAd.EmailAddress].Value.ToString()))
                        {
                            directoryEntry.Properties[UserPropertiesAd.EmailAddress].Value = email;
                            checkForUpdate = true;
                        }
                    }
                    else
                    {
                        directoryEntry.Properties[UserPropertiesAd.EmailAddress].Value = email;
                        checkForUpdate = true;
                    }

                    //Lấy một số thông tin mà HRM không có
                    //mobile
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.Mobile))
                    {
                        userInfo.mobile = directoryEntry.Properties[UserPropertiesAd.Mobile].Value.ToString();
                    }
                    //homePhone
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.Homephone))
                    {
                        userInfo.homePhone = directoryEntry.Properties[UserPropertiesAd.Homephone].Value.ToString();
                    }
                    //cn
                    if (directoryEntry.Properties.Contains(UserPropertiesAd.ContainerName))
                    {
                        userInfo.CN = directoryEntry.Properties[UserPropertiesAd.ContainerName].Value.ToString();
                    }

                    //prop.Value = -1;
                    //directoryEntry.Properties[UserPropertiesAd.Department].Value = "Day la phong ban moi";
                    //directoryEntry.CommitChanges();

                    if (checkForUpdate)
                    {
                        try
                        {
                            directoryEntry.CommitChanges();
                            result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
                        }
                        catch (Exception e)
                        {
                            result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Update dữ liệu " + model.TenDangNhap + " vào AD không thành công. Error: " + e.Message);
                        }
                    }
                    else
                    {
                        result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "User " + model.TenDangNhap + " không có thông tin cần update.");
                    }

                    result.UserInfo = userInfo;
                }
                else
                {
                    result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Không xác định được các Attributes của User " + model.TenDangNhap);
                }

                listResult.Add(result);
            }

            return listResult;
        }

        public ApiResultAd CreateUser(UserInfoAd user, string pw)
        {
            _logger.Information("PasswordChangeProvider.CreateUser");

            var result = new ApiResultAd { UserInfo = null };

            string fixedUsername = "";
            string username = user.sAMAccountName;
            var principalContext = AcquirePrincipalContext();

            //Kiểm tra user đã tồn tại chưa
            bool check = true;
            int i = 0;
            while (check)
            {
                fixedUsername = FixUsernameWithDomain(username);
                var up = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);
                if (up != null)
                {
                    i++;
                    username = user.sAMAccountName + i;
                    up.Dispose();
                }
                else
                {
                    check = false;
                }
            }

            //Cach 1 Create User
            //OU: OU=Company Structure,DC=baonx,DC=com
            DirectoryEntry ouEntry = new DirectoryEntry("LDAP://OU=Company Structure,DC=baonx,DC=com");

            DirectoryEntry childEntry = ouEntry.Children.Add("CN=" + username, "user");
            childEntry.Properties[UserPropertiesAd.UserPrincipalName].Value = fixedUsername;
            childEntry.Properties[UserPropertiesAd.LoginName].Value = username; //SamAccountName
            childEntry.Properties[UserPropertiesAd.Name].Value = username;
            childEntry.Properties[UserPropertiesAd.LastName].Value = user.givenName;//sn, Surname, LastName = Ho va ten dem
            childEntry.Properties[UserPropertiesAd.FirstName].Value = user.sn;//GivenName, FirstName = Ten
            childEntry.Properties[UserPropertiesAd.DisplayName].Value = user.displayName;//GivenName, FirstName
            childEntry.Properties[UserPropertiesAd.EmailAddress].Value = username + "@haiphatland.com.vn";
            childEntry.Properties[UserPropertiesAd.TelePhoneNumber].Value = user.telephoneNumber;
            childEntry.Properties[UserPropertiesAd.Description].Value = user.description;
            //Thong tin phong ban
            childEntry.Properties[UserPropertiesAd.EmployeeId].Value = "MaNhanVien";
            childEntry.Properties[UserPropertiesAd.Department].Value = "Phong Ban";
            childEntry.Properties[UserPropertiesAd.Title].Value = "Chuc Danh";
            //Enable user
            childEntry.Properties[UserPropertiesAd.UserAccountControl].Value = 66048;
            childEntry.Properties[UserPropertiesAd.PwdLastSet].Value = 0;
            childEntry.Properties[UserPropertiesAd.AccountExpires].Value = "9223372036854775807";

            childEntry.CommitChanges();

            ouEntry.CommitChanges();

            childEntry.Invoke("SetPassword", new object[] { pw });
            childEntry.CommitChanges();

            //Cach 2 Create User
            //Add user vao Group
            string groupName = "Employees";
            GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, groupName);
            group.Members.Add(principalContext, IdentityType.UserPrincipalName, fixedUsername);
            group.Save();

            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);
            var userInfo = new UserInfoAd
            {
                isLocked = userPrincipal.IsAccountLockedOut(),
                userPrincipalName = userPrincipal.UserPrincipalName,
                sAMAccountName = userPrincipal.SamAccountName,
                name = userPrincipal.Name,
                sn = userPrincipal.Surname,
                givenName = userPrincipal.GivenName,
                displayName = userPrincipal.DisplayName,
                mail = userPrincipal.EmailAddress,
                telephoneNumber = userPrincipal.VoiceTelephoneNumber,
                description = userPrincipal.Description
            };

            //Update lại mot so thong tin cua nhan su
            if (userPrincipal.GetUnderlyingObject() is DirectoryEntry directoryEntry)
            {
                //Phong ban
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Department))
                {
                    userInfo.department = directoryEntry.Properties[UserPropertiesAd.Department].Value.ToString();
                }
                //Chuc Vu
                if (directoryEntry.Properties.Contains(UserPropertiesAd.Title))
                {
                    userInfo.title = directoryEntry.Properties[UserPropertiesAd.Title].Value.ToString();
                }
                //Ma Nhan Vien
                if (directoryEntry.Properties.Contains(UserPropertiesAd.EmployeeId))
                {
                    userInfo.employeeID = directoryEntry.Properties[UserPropertiesAd.EmployeeId].Value.ToString();
                }
                //CN
                if (directoryEntry.Properties.Contains(UserPropertiesAd.ContainerName))
                {
                    userInfo.CN = directoryEntry.Properties[UserPropertiesAd.ContainerName].Value.ToString();
                }

                //distinguishedName (CN=baonx,OU=Company Structure,DC=baonx,DC=com)
                if (directoryEntry.Properties.Contains(UserPropertiesAd.DistinguishedName))
                {
                    //directoryEntry.Properties[UserPropertiesAd.DistinguishedName].Value = "OU=Company Structure,DC=baonx,DC=com";
                    userInfo.distinguishedName = directoryEntry.Properties[UserPropertiesAd.DistinguishedName].Value.ToString();
                }

                //memberOf: (CN=Employees,CN=Users,DC=baonx,DC=com)
                if (directoryEntry.Properties.Contains(UserPropertiesAd.MemberOf))
                {
                    userInfo.memberOf = directoryEntry.Properties[UserPropertiesAd.MemberOf].Value.ToString();
                }

                //objectCategory: (CN=Person,CN=Schema,CN=Configuration,DC=baonx,DC=com)
                if (directoryEntry.Properties.Contains(UserPropertiesAd.ObjectCategory))
                {
                    userInfo.objectCategory = directoryEntry.Properties[UserPropertiesAd.ObjectCategory].Value.ToString();
                }

                result.UserInfo = userInfo;

                directoryEntry.Dispose();
            }
            else
            {
                result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Không xác định được các Attributes của User");
            }

            principalContext.Dispose();
            userPrincipal.Dispose();
            group.Dispose();

            result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
            result.UserInfo = userInfo;

            return result;
        }

        public ApiResultAd CreateUser2(UserInfoAd user, string pw)
        {
            _logger.Information("PasswordChangeProvider.CreateUser");

            var result = new ApiResultAd { UserInfo = null };

            string fixedUsername = "";
            string username = user.sAMAccountName;
            var principalContext = AcquirePrincipalContext();

            //Kiểm tra user đã tồn tại chưa
            bool check = true;
            int i = 0;
            while (check)
            {
                fixedUsername = FixUsernameWithDomain(username);
                var up = UserPrincipal.FindByIdentity(principalContext, _idType, fixedUsername);
                if (up != null)
                {
                    i++;
                    username = user.sAMAccountName + i;
                    up.Dispose();
                }
                else
                {
                    check = false;
                }
            }

            //Cach 1 Create User
            //OU: OU=Company Structure,DC=baonx,DC=com
            //DirectoryEntry ouEntry = new DirectoryEntry("LDAP://OU=Company Structure,DC=baonx,DC=com");
            //for (int j = 0; j < 2; j++)
            //{
            //    try
            //    {
            //        DirectoryEntry childEntry = ouEntry.Children.Add("CN=TestUser" + i, "user");
            //        childEntry.CommitChanges();
            //        ouEntry.CommitChanges();
            //        childEntry.Invoke("SetPassword", new object[] { "password" });
            //        childEntry.CommitChanges();
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}

            //Cach 2 Create User
            var userPrincipal = new UserPrincipal(principalContext)
            {
                UserPrincipalName = fixedUsername,
                SamAccountName = username,
                Name = username,
                Surname = user.sn,
                GivenName = user.givenName,
                DisplayName = user.displayName,
                EmailAddress = username + "@haiphatland.com.vn",
                VoiceTelephoneNumber = user.telephoneNumber,
                Description = user.description
            };
            userPrincipal.SetPassword(pw);
            userPrincipal.Enabled = true;
            userPrincipal.PasswordNeverExpires = true;
            userPrincipal.Save();

            //Add user vao Group
            string groupName = "Employees";
            GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, groupName);
            group.Members.Add(principalContext, IdentityType.UserPrincipalName, fixedUsername);
            group.Save();

            var userInfo = new UserInfoAd
            {
                isLocked = userPrincipal.IsAccountLockedOut(),
                userPrincipalName = userPrincipal.UserPrincipalName,
                sAMAccountName = userPrincipal.SamAccountName,
                name = userPrincipal.Name,
                sn = userPrincipal.Surname,
                givenName = userPrincipal.GivenName,
                displayName = userPrincipal.DisplayName,
                mail = userPrincipal.EmailAddress,
                telephoneNumber = userPrincipal.VoiceTelephoneNumber,
                description = userPrincipal.Description
            };

            //Update lại mot so thong tin cua nhan su
            bool checkForUpdate = false;
            if (userPrincipal.GetUnderlyingObject() is DirectoryEntry directoryEntry)
            {
                //try
                //{
                //    DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDn);
                //    dirEntry.Properties["member"].Add(userDn);
                //    dirEntry.CommitChanges();
                //    dirEntry.Close();
                //}
                //catch (System.DirectoryServices.DirectoryServicesCOMException E)
                //{
                //    //doSomething with E.Message.ToString();
                //}

                //department: Chi nhánh/Phòng ban
                directoryEntry.Properties[UserPropertiesAd.Department].Value = "Ten phong ban";
                userInfo.department = "Ten phong ban";
                //title = Chức danh
                directoryEntry.Properties[UserPropertiesAd.Title].Value = "Chuc danh";
                userInfo.title = "Chuc vu";
                //employeeID mã nhân viên
                directoryEntry.Properties[UserPropertiesAd.EmployeeId].Value = "Ma Nhan Vien";
                userInfo.employeeID = "ma nha vien";

                //CN
                if (directoryEntry.Properties.Contains(UserPropertiesAd.ContainerName))
                {
                    userInfo.CN = directoryEntry.Properties[UserPropertiesAd.ContainerName].Value.ToString();
                }

                //distinguishedName (CN=baonx,OU=Company Structure,DC=baonx,DC=com)
                if (directoryEntry.Properties.Contains(UserPropertiesAd.DistinguishedName))
                {
                    //directoryEntry.Properties[UserPropertiesAd.DistinguishedName].Value = "OU=Company Structure,DC=baonx,DC=com";
                    userInfo.distinguishedName = directoryEntry.Properties[UserPropertiesAd.DistinguishedName].Value.ToString();
                }

                //memberOf: (CN=Employees,CN=Users,DC=baonx,DC=com)
                if (directoryEntry.Properties.Contains(UserPropertiesAd.MemberOf))
                {
                    userInfo.memberOf = directoryEntry.Properties[UserPropertiesAd.MemberOf].Value.ToString();
                }

                //objectCategory: (CN=Person,CN=Schema,CN=Configuration,DC=baonx,DC=com)
                if (directoryEntry.Properties.Contains(UserPropertiesAd.ObjectCategory))
                {
                    userInfo.objectCategory = directoryEntry.Properties[UserPropertiesAd.ObjectCategory].Value.ToString();
                }

                try
                {
                    directoryEntry.CommitChanges();
                    result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
                }
                catch (Exception e)
                {
                    result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Update dữ liệu: " + e.Message);
                }

                result.UserInfo = userInfo;

                directoryEntry.Close();
                directoryEntry.Dispose();
            }
            else
            {
                result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Không xác định được các Attributes của User");
            }

            principalContext.Dispose();
            userPrincipal.Dispose();

            result.Errors = new ApiErrorItem(ApiErrorCode.Generic, "Successful");
            result.UserInfo = userInfo;

            return result;
        }

        /// <inheritdoc />
        public ApiErrorItem? PerformPasswordChange(string username, string currentPassword, string newPassword)
        {
            try
            {
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

                //Check User thuộc group nào, có một số Groups đặc biệt, không cho phép user đổi pass bằng tool này
                var item = ValidateGroups(userPrincipal);
                if (item != null) return item;

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

                if (_options.RestrictedADGroups != null)
                    if (groups.Any(x => _options.RestrictedADGroups.Contains(x.Name)))
                    {
                        return new ApiErrorItem(ApiErrorCode.ChangeNotPermitted,
                            "The User " + userPrincipal.SamAccountName + " principal is listed as restricted.");
                    }

                var valueReturn = groups?.Any(x => _options.AllowedADGroups?.Contains(x.Name) == true) == true
                    ? null
                    : new ApiErrorItem(ApiErrorCode.ChangeNotPermitted, "The User " + userPrincipal.SamAccountName + " principal is not listed as allowed");

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
                _logger.Debug("Goi method userPrincipal.SetPassword()");
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
            try
            {
                return new PrincipalContext(ContextType.Domain, domain, _options.LdapUsername, _options.LdapPassword);
            }
            catch (Exception e)
            {
                _logger.Warning($"Lỗi call AcquirePrincipalContext: " + e.Message);
                return null;
            }
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
            _logger.Warning("PasswordChangeProvider.AcquirePrincipalContext: " + JsonConvert.SerializeObject(_options));

            var domain = $"{_options.LdapHostnames.First()}:{_options.LdapPort}";
            //_logger.Warning($"Not using AutomaticContext  {domain}");

            return new PrincipalContext(
                ContextType.Domain,
                domain,
                username,
                pw);
        }

        private UserInfoAd ConvertUserProfiles(DirectoryEntry dirEntry)
        {
            UserInfoAd user = new UserInfoAd();

            //CN: Hiện thị ContainerName
            if (dirEntry.Properties.Contains(UserPropertiesAd.ContainerName))
            {
                user.CN = dirEntry.Properties[UserPropertiesAd.ContainerName].Value.ToString();
            }
            //user có dạng admin@baonx.com
            if (dirEntry.Properties.Contains(UserPropertiesAd.UserPrincipalName))
            {
                user.userPrincipalName = dirEntry.Properties[UserPropertiesAd.UserPrincipalName].Value.ToString();// = fixedUsername;
            }

            if (dirEntry.Properties.Contains(UserPropertiesAd.LoginName))
            {
                user.sAMAccountName = dirEntry.Properties[UserPropertiesAd.LoginName].Value.ToString();//username; //SamAccountName
            }

            if (dirEntry.Properties.Contains(UserPropertiesAd.Name))
            {
                user.name = dirEntry.Properties[UserPropertiesAd.Name].Value.ToString();//username;
            }
            //user.givenName;//sn, Surname, LastName = Ho va ten dem
            if (dirEntry.Properties.Contains(UserPropertiesAd.LastName))
            {
                user.givenName = dirEntry.Properties[UserPropertiesAd.LastName].Value.ToString();
            }
            //user.sn;//GivenName, FirstName = Ten
            if (dirEntry.Properties.Contains(UserPropertiesAd.FirstName))
            {
                user.sn = dirEntry.Properties[UserPropertiesAd.FirstName].Value.ToString();
            }
            //user.displayName;//GivenName, FirstName
            if (dirEntry.Properties.Contains(UserPropertiesAd.DisplayName))
            {
                user.displayName = dirEntry.Properties[UserPropertiesAd.DisplayName].Value.ToString();
            }
            //username + "@haiphatland.com.vn";
            if (dirEntry.Properties.Contains(UserPropertiesAd.EmailAddress))
            {
                user.mail = dirEntry.Properties[UserPropertiesAd.EmailAddress].Value.ToString();
            }

            if (dirEntry.Properties.Contains(UserPropertiesAd.TelePhoneNumber))
            {
                user.telephoneNumber = dirEntry.Properties[UserPropertiesAd.TelePhoneNumber].Value.ToString();//user.telephoneNumber;
            }

            if (dirEntry.Properties.Contains(UserPropertiesAd.Description))
            {
                user.description = dirEntry.Properties[UserPropertiesAd.Description].Value.ToString();//user.description;
            }

            //Thong tin phong ban
            //employeeID mã nhân viên
            if (dirEntry.Properties.Contains(UserPropertiesAd.EmployeeId))
            {
                user.employeeID = dirEntry.Properties[UserPropertiesAd.EmployeeId].Value.ToString();//"MaNhanVien";
            }
            //department: Chi nhánh/Phòng ban
            if (dirEntry.Properties.Contains(UserPropertiesAd.Department))
            {
                user.department = dirEntry.Properties[UserPropertiesAd.Department].Value.ToString();//"Phong Ban";
            }
            //title = Chức danh
            if (dirEntry.Properties.Contains(UserPropertiesAd.Title))
            {
                user.title = dirEntry.Properties[UserPropertiesAd.Title].Value.ToString();//"Chuc Danh";
            }

            //OU
            if (dirEntry.Properties.Contains(UserPropertiesAd.DistinguishedName))
            {
                user.distinguishedName = dirEntry.Properties[UserPropertiesAd.DistinguishedName].Value.ToString();
            }

            //MemberOf
            if (dirEntry.Properties.Contains(UserPropertiesAd.MemberOf))
            {
                user.memberOf = dirEntry.Properties[UserPropertiesAd.MemberOf].Value.ToString();
            }

            //Object Category
            if (dirEntry.Properties.Contains(UserPropertiesAd.ObjectCategory))
            {
                user.objectCategory = dirEntry.Properties[UserPropertiesAd.ObjectCategory].Value.ToString();
            }


            return user;
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
