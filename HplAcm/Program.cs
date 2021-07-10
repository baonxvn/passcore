using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;

namespace HplAcm
{
    static class Program
    {
        public static IConfigurationRoot configuration;
        private const string AppSettingsSectionName = "AppSettings";

        private static IOptions<PasswordChangeOptions> _options;
        private static ILogger _logger;
        private static IPasswordChangeProvider _passwordChangeProvider;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //var services = new ServiceCollection();
            //ConfigureServices(services);

            //using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            //{
            //    var formMain = serviceProvider.GetRequiredService<FormMain>();
            //    Application.Run(new FormMain(_logger, _options, _passwordChangeProvider));
            //}

            using IHost host = CreateHostBuilder().Build();
            var services = host.Services;
            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            _logger = provider.GetRequiredService<ILogger>();
            _passwordChangeProvider = provider.GetRequiredService<IPasswordChangeProvider>();
            _options = provider.GetRequiredService<IOptions<PasswordChangeOptions>>();
            var formMain = provider.GetRequiredService<FormMain>();
            Application.Run(new FormMain(_logger, _options, _passwordChangeProvider));


            //Cách call 2 Dependency Injection
            //var servicesCol = new ServiceCollection();
            //ConfigureServices(servicesCol);
            //using (ServiceProvider serviceProvider = servicesCol.BuildServiceProvider())
            //{
            //    var formMain = serviceProvider.GetRequiredService<FormMain>();
            //    Application.Run(new FormMain(_logger, _options, _passwordChangeProvider));
            //}
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            services.
                Configure<PasswordChangeOptions>(configuration.GetSection(AppSettingsSectionName))
                .AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>()
                .AddSingleton((ILogger)new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                            (utcDateTime, wt) => wt.File($"logs/acm-log-{utcDateTime}.txt"))
                        .CreateLogger());

        }

        static IHostBuilder CreateHostBuilder()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            return Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                    services
                        .Configure<PasswordChangeOptions>(configuration.GetSection(AppSettingsSectionName))
                        .AddScoped<FormMain>()
                        .AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>()
                        .AddSingleton((ILogger)new LoggerConfiguration()
                            .MinimumLevel.Verbose()
                            .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                                (utcDateTime, wt) => wt.File($"logs/acm-log-{utcDateTime}.txt"))
                            .CreateLogger()));

        }
    }
}
