// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace Unosquare.PassCore.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Unosquare.PassCore.Web.MdaemonServices;
    using System.Threading.Tasks;
    using Unosquare.PassCore.Web.Services;
    using Unosquare.PassCore.Common;
    using Unosquare.PassCore.Web.Models;
    using Serilog;
    using Microsoft.Extensions.Options;

    [Route("api/[controller]")]
    [ApiController]
    public class ApiCommonController : ControllerBase
    {
        private readonly IServiceCommon _serviceCommon;
        private readonly ILogger _logger;
        private readonly ClientSettings _options;

        public ApiCommonController(IServiceCommon serviceCommon, ILogger logger, IOptions<ClientSettings> options)
        {
            _serviceCommon = serviceCommon;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Tạo AD User, Email, User HRM.
        /// </summary>
        /// <param name="maNhanVien"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CreateUserTheoMaNhanVien")]
        public async Task<string> CreateUserTheoMaNhanVien(string maNhanVien)
        {
            _logger.Information("START ApiCommonController.CreateUserTheoMaNhanVien. maNhanVien=" + maNhanVien);

            var res = await _serviceCommon.CreateUserTheoMaNhanVien(maNhanVien);
            return JsonConvert.SerializeObject(res);
        }

        // GET: api/<ApiCommonController>
        [HttpGet]
        public string Get()
        {
            var res = _serviceCommon.TestApi();

            var strs = new string[] { "value1", res };
            return JsonConvert.SerializeObject(strs);
        }

        // GET api/<ApiCommonController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ApiCommonController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ApiCommonController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ApiCommonController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
