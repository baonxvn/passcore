using System;
using System.IO;
using DependencyInjectionConsoleApp.MyServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DependencyInjectionConsoleApp
{
    public class Startup
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>();


        public void ConfigureServices(IServiceCollection services)
        {
           

            //services.Configure<ClientSettings>(Configuration.GetSection(nameof(ClientSettings)));
            //services.Configure<WebSettings>(Configuration.GetSection(nameof(WebSettings)));

    

            //services.Configure<PasswordChangeOptions>(Configuration.GetSection(AppSettingsSectionName));
            //services.AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>();
            //services.AddSingleton((ILogger)new LoggerConfiguration()
            //                    .MinimumLevel.Verbose()
            //                    .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //                        (utcDateTime, wt) => wt.File($"logs/LDAP_Win-log-{utcDateTime}.txt"))
            //                    .CreateLogger());
         
        }
    }
}