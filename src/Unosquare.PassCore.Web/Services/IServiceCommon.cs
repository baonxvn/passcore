
namespace Unosquare.PassCore.Web.Services
{
    using System.Threading.Tasks;
    using Unosquare.PassCore.Web.Models;

    public interface IServiceCommon
    {

        public const string MailDomain = "company.test";
        public const string MailUser = "company.test";
        public const string MailPass = "company.test";

        public string TestApi();

        public Task<ApiResult> CreateUserTheoMaNhanVien(string maNhanVien);
    }
}