using System;
using Hpl.Common.MyService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;

namespace SyncOosToAd
{
    public class Startup
    {
        private const string AppSettingsSectionName = "AppSettings";

        private IConfiguration _configuration;
        private IServiceProvider _provider;

        //IConfigurationRoot Configuration { get; }
        public IConfiguration Configuration => _configuration;
        // access the built service pipeline
        public IServiceProvider Provider => _provider;

        // access the built configuration

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            //var builder = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json");

            //_configuration = builder.Build();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args);
        }

        public void ConfigureServices()
        {
            // instantiate
            IServiceCollection services = new ServiceCollection();

            //services.AddLogging();
            //services.AddSingleton<IConfigurationRoot>(configuration);
            services.AddSingleton<IConfiguration>(_configuration);
            services.AddSingleton<IMyService, MyService>();

            //Cac service su dung
            services.Configure<PasswordChangeOptions>(Configuration.GetSection(AppSettingsSectionName));
            services.AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>();
            services.AddSingleton((ILogger)new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                    (utcDateTime, wt) => wt.File($"logs/LDAP_Win-log-{utcDateTime}.txt"))
                .CreateLogger());


            _provider = services.BuildServiceProvider();
        }
    }
}