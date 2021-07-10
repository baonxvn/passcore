// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace Unosquare.PassCore.Web.Controllers
{
    using Hpl.HrmDatabase.Services;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using Unosquare.PassCore.Web.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class AbpHplController : ControllerBase
    {
        [HttpGet]
        [Route("UpdateBranch")]
        public string UpdateBranch()
        {
            AbpServices.UpdateBranch();

            return JsonConvert.SerializeObject("OK!");
        }

        [HttpGet]
        [Route("GetAllLogNhanVien")]
        public string GetAllLogNhanVien()
        {
            var resrult = new ApiResult
            {
                Payload = AbpServices.GetAllLogNhanVien()
            };

            return JsonConvert.SerializeObject(resrult);
        }

        [HttpGet]
        [Route("GetAllPhongBan")]
        public string GetAllPhongBan()
        {
            var resrult = new ApiResult
            {
                Payload = AbpServices.GetListPhongBan()
            };

            return JsonConvert.SerializeObject(resrult);
        }

        // GET: api/<AbpHplController>
        [HttpGet]
        public IEnumerable<string> Get()
        {

            return new string[] { "value1", "value2" };
        }

        // GET api/<AbpHplController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AbpHplController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AbpHplController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AbpHplController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
