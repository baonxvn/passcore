namespace Unosquare.PassCore.Web.Controllers
{
    using Common;
    using Hpl.HrmDatabase.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Models;
    using Newtonsoft.Json;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Unosquare.PassCore.Web.MdaemonServices;

    [Route("api/[controller]")]
    [ApiController]
    public class HrmController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ClientSettings _options;
        private readonly IPasswordChangeProvider _passwordChangeProvider;

        public HrmController(ILogger logger, IOptions<ClientSettings> optionsAccessor, IPasswordChangeProvider passwordChangeProvider)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
            _passwordChangeProvider = passwordChangeProvider;
        }

        // GET: api/<HrmController>
        [HttpGet]
        //public IEnumerable<string> Get()
        public string Get()
        {
            //return new string[] { "value1", "value2" };
            return JsonConvert.SerializeObject(_options);
        }

        // GET api/<HrmController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        [Route("GetUserInfo")]
        public string GetUserInfo(string username)
        {
            var result = new ApiResult();

            var obj = UserService.GetNhanVienByUsername(username);

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại user."));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetUserInfoTheoMaNhanVien")]
        public string GetUserInfoTheoMaNhanVien(string maNhanVien)
        {
            var result = new ApiResult();

            var obj = UserService.GetNhanVienByMaNhanVien(maNhanVien);

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại user."));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("FixUsernameNhanVien")]
        public async Task<string> FixUsernameNhanVien(string maNhanVien)
        {
            var result = new ApiResult();
            try
            {
                if (string.IsNullOrEmpty(maNhanVien))
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Không xác định được mã"));
                    return JsonConvert.SerializeObject(result);
                }

                List<object> listObjs = new List<object>();

                //var obj = UserService.FixUsernameNhanVien(maNhanVien);
                //listObjs.Add(obj);
                ////Tạo email
                //CreateUserInput input = new CreateUserInput();
                //input.Domain = "haiphatland.com.vn";
                //input.Username = obj.TenDangNhap;
                //input.FirstName = UsernameGenerator.ConvertToUnSign(obj.Ten);
                //input.LastName = UsernameGenerator.ConvertToUnSign(obj.Ho);
                //input.FullName = UsernameGenerator.ConvertToUnSign(obj.Ho + " " + obj.Ten);
                //input.Password = "Hpl@123";
                //input.AdminNotes = "Tạo từ tool, time: " + DateTime.Now.ToString("G");
                //input.MailList = "";
                //input.Group = "";
                //var res = await MdaemonXmlApi.CreateUser(input);
                //listObjs.Add(res);
                //result.Payload = listObjs;
                //result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Successful"));
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: " + e.Message));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("FixUsernameNhanVien")]
        public async Task<string> FixNhanVienTheoUsername(string userName)
        {
            var result = new ApiResult();
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: Không xác định được mã"));
                    return JsonConvert.SerializeObject(result);
                }

                List<object> listObjs = new List<object>();

                var obj = UserService.FixNhanVienTheoUsername(userName);
                listObjs.Add(obj);
                //Tạo email
                CreateUserInput input = new CreateUserInput();
                input.Domain = "haiphatland.com.vn";
                input.Username = obj.TenDangNhap;
                input.FirstName = UsernameGenerator.ConvertToUnSign(obj.Ten);
                input.LastName = UsernameGenerator.ConvertToUnSign(obj.Ho);
                input.FullName = UsernameGenerator.ConvertToUnSign(obj.Ho + " " + obj.Ten);
                input.Password = "Hpl@123";
                input.AdminNotes = "Tạo từ tool, time: " + DateTime.Now.ToString("G");
                input.MailList = "";
                input.Group = "";
                var res = await MdaemonXmlApi.CreateUser(input);
                listObjs.Add(res);
                result.Payload = listObjs;
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Successful"));
            }
            catch (Exception e)
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, "Lỗi: " + e.Message));
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Lấy thông Phong Ban cấp 1 của Hồ hơ nhân sự (Theo Mã Nhân Viên)
        /// </summary>
        /// <param name="maNhanVien"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPhongBanCap1CuaNhanVien")]
        public string GetPhongBanCap1CuaNhanVien(string maNhanVien)
        {
            var result = new ApiResult();

            var obj = UserService.GetPhongBanCap1CuaNhanVien(maNhanVien);

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại ID của phòng."));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetAllNhanVienCuaPhongBan")]
        public string GetAllNhanVienCuaPhongBan(int id)
        {
            var result = new ApiResult();

            var obj = UserService.GetAllNhanVienCuaPhongBan(id);

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại ID của phòng."));
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetAllNhanVienTheoMa")]
        public string GetAllNhanVienTheoMa(string listMaNvs)
        {
            var result = new ApiResult();
            listMaNvs = listMaNvs.Trim().Replace(" ", "");
            List<string> listMaNhanVien = listMaNvs.Split(",").ToList();
            if (listMaNvs.Any())
            {
                var obj = UserService.GetAllNhanVienTheoMa(listMaNhanVien);

                if (obj != null)
                {
                    result.Payload = obj;
                }
                else
                {
                    result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại."));
                }
            }
            else
            {
                result.Payload = "";
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Lỗi: Chuỗi nhập vào không đúng"));
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Lấy dang sách phong ban chính thức (trong DB Mapping)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllHplPhongBan")]
        public string GetAllHplPhongBan()
        {
            var result = new ApiResult();

            var obj = UserService.GetAllHplPhongBan();

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại ID của phòng."));
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Lấy dang sách phong ban con (tính cả ID của nó, trong DB HRM)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllChildrenAndSelf")]
        public string GetAllChildrenAndSelf(int id)
        {
            var result = new ApiResult();

            var obj = UserService.GetAllChildrenAndSelf(id);

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại ID của phòng."));
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Lấy tất cả dang sách phong ban con và cha
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllChildrenAndParents")]
        public string GetAllChildrenAndParents(int id)
        {
            var result = new ApiResult();

            var obj = UserService.GetAllChildrenAndParents(id);

            if (obj != null)
            {
                result.Payload = obj;
            }
            else
            {
                result.Errors.Add(new ApiErrorItem(ApiErrorCode.UserNotFound, "Không tồn tại ID của phòng."));
            }

            return JsonConvert.SerializeObject(result);
        }

        // POST api/<HrmController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HrmController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HrmController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
