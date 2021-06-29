// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace Unosquare.PassCore.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Serilog;
    using Unosquare.PassCore.Common;
    using Unosquare.PassCore.Web.Models;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swan.Net;
    using Hpl.HrmDatabase.Services;

    using System.Threading.Tasks;
    [Route("api/[controller]")]
    [ApiController]
    public class AdApiController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ClientSettings _options;
        private readonly IPasswordChangeProvider _passwordChangeProvider;

        private const string Domain = "haiphatland.local";
        private const string PwDefault = "Hpl@123";

        public AdApiController(ILogger logger, IOptions<ClientSettings> optionsAccessor, IPasswordChangeProvider passwordChangeProvider)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
            _passwordChangeProvider = passwordChangeProvider;
        }

        // GET: api/<AdApiController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public string GetAllUsers()
        {
            _logger.Information("START PasswordController.GetAllUsers: ");
            ApiResult result = new ApiResult();
            try
            {
                var obj = _passwordChangeProvider.GetAllUsers();
                if (obj != null)
                {
                    result.Payload = obj;
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Successful"));
                }
                else
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user."));
                }
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user. Lỗi: " + e.Message));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetUserInfo")]
        public string GetUserInfo(string username, string pw)
        {
            ApiResult result = new ApiResult();
            _logger.Information("START PasswordController.GetUserInfo: " + username);

            try
            {
                var obj = _passwordChangeProvider.GetUserInfo(username, pw);
                if (obj != null)
                {
                    result.Payload = obj;
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Successful"));
                }
                else
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user."));
                }
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user. Lỗi: " + e.Message));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetUserNewUserFormAd")]
        public string GetUserNewUserFormAd(string username)
        {
            ApiResult result = new ApiResult();
            _logger.Information("START PasswordController.GetUserNewUserFormAd: " + username);

            try
            {
                var obj = _passwordChangeProvider.GetUserNewUserFormAd(username);
                if (obj != null)
                {
                    result.Payload = obj;
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Successful"));
                }
                else
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user."));
                }
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user. Lỗi: " + e.Message));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("CreateUser")]
        public string CreateUser(string username, string pw)
        {
            ApiResult result = new ApiResult();
            _logger.Information("START PasswordController.CreateUser: " + username);

            try
            {
                UserInfoAd user = new UserInfoAd
                {
                    userPrincipalName = "",
                    sAMAccountName = username,
                    name = "",
                    sn = "Bao",
                    givenName = "Nguyen Xuan",
                    displayName = "Nguyen Xuan Bao",
                    mail = "",
                    telephoneNumber = "0912345678",
                    department = "Ban Cong Nghe",
                    title = "Giám đốc",
                    employeeID = "VP-291",
                    description = "Tạo từ tool thần thánh"
                };

                var obj = _passwordChangeProvider.CreateUser(user, pw);
                if (obj != null)
                {
                    result.Payload = obj;
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Successful"));
                }
                else
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user."));
                }
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không xác định được user. Lỗi: " + e.Message));
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Đồng bộ phòng ban sang AD
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SyncUserPhongBanToAd")]
        public string SyncUserPhongBanToAd(int id)
        {
            _logger.Information("START HrmController.SyncUserPhongBanToAd: ");
            var result = new ApiResult();

            var listNvs = UserService.GetAllNhanVienCuaPhongBan(id);
            var obj = _passwordChangeProvider.UpdateUserInfo(listNvs);

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Có một số lỗi xảy ra."));
            }

            return JsonConvert.SerializeObject(result);
        }

        // GET api/<AdApiController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AdApiController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AdApiController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AdApiController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
