using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SyncOosToAd.MyService;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;

namespace SyncOosToAd
{
    public class Startup
    {
        private const string AppSettingsSectionName = "AppSettings";

        IConfigurationRoot Configuration { get; }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<IConfigurationRoot>(Configuration);
            services.AddSingleton<IMyService, MyService.MyService>();

            //Cac service su dung
            services.Configure<PasswordChangeOptions>(Configuration.GetSection(AppSettingsSectionName));
            services.AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>();
            services.AddSingleton((ILogger)new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                    (utcDateTime, wt) => wt.File($"logs/LDAP_Win-log-{utcDateTime}.txt"))
                .CreateLogger());
        }
    }
}