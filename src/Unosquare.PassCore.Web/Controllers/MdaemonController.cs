// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace Unosquare.PassCore.Web.Controllers
{
    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MdaemonServices;

    [Route("api/[controller]")]
    [ApiController]
    public class MdaemonController : ControllerBase
    {
        // GET: api/<MdaemonController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Server connecting..." };
        }

        [HttpGet]
        [Route("GetUserInfo")]
        public async Task<string> GetUserInfo(string username)
        {
            var res = await MdaemonXmlApi.GetUserInfo(username);
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        [Route("CreateUserTheoMaNhanVien")]
        public async Task<string> CreateUserTheoMaNhanVien(string maNhanVien)
        {
            var res = await MdaemonXmlApi.CreateUserTheoMaNhanVien(maNhanVien);
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        [Route("CreateUser")]
        public async Task<string> CreateUser()
        {
            var listUser = UtilityHelpers.CreateUserDemo();

            var res = await MdaemonXmlApi.CreateUser(listUser);
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        [Route("GetDomainList")]
        public async Task<string> GetDomainList()
        {
            var res = await MdaemonXmlApi.GetDomainList();
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        [Route("MailingListCountUsers")]
        public async Task<string> MailingListCountUsers(string listName)
        {
            var res = await MdaemonXmlApi.MailingListCountUsers(listName);
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        [Route("MailingGetListInfo")]
        public async Task<string> MailingGetListInfo(string listName)
        {
            var res = await MdaemonXmlApi.MailingGetListInfo(listName);
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        [Route("MailingCreateList")]
        public async Task<string> MailingCreateList(string listName)
        {
            var res = await MdaemonXmlApi.MailingCreateList(listName, "Đây là mô tả");
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        [Route("MailingUpdateList")]
        public async Task<string> MailingUpdateList(string listName)
        {
            var res = await MdaemonXmlApi.MailingUpdateList(listName, "Đây là mô tả");
            return JsonConvert.SerializeObject(res);
        }
        // GET api/<MdaemonController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<MdaemonController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<MdaemonController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MdaemonController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
