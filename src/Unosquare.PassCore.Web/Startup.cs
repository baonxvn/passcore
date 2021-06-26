namespace Unosquare.PassCore.Web
{
    using System;
    using Common;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Models;
    using Serilog;
    using PasswordProvider;
    //using Zyborg.PassCore.PasswordProvider.LDAP;

    //#if DEBUG
    //    using Helpers;
    //#elif PASSCORE_LDAP_PROVIDER
    //    using Zyborg.PassCore.PasswordProvider.LDAP;
    //#else
    //    using PasswordProvider;
    //#endif

    /// <summary>
    /// Represents this application's main class.
    /// </summary>
    public class Startup
    {
        private const string AppSettingsSectionName = "AppSettings";
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// This class gets instantiated by the Main method. The hosting environment gets provided via DI.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Application's entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args) =>
            CreateWebHostBuilder(args).Build().Run();

        /// <summary>
        /// Creates the web host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// All arguments are provided through dependency injection.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            //BaoNX: Enable Cross-Origin Requests (CORS) in ASP.NET Core
            //https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0
            //https://stackoverflow.com/questions/66101488/net-5-web-api-cors-localhost
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(name: MyAllowSpecificOrigins,
            //        builder =>
            //        {
            //            builder.WithOrigins("https://id.haiphatland.com.vn",
            //                "https://localhost",
            //                "https://localhost:44352",
            //                "http://172.168.1.150")
            //                .AllowAnyHeader()
            //                .AllowAnyMethod()
            //                .AllowCredentials();
            //        });
            //});
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });
            services.AddControllers();
            //End Enable Cross-Origin Requests (CORS) in ASP.NET Core

            services.Configure<ClientSettings>(Configuration.GetSection(nameof(ClientSettings)));
            services.Configure<WebSettings>(Configuration.GetSection(nameof(WebSettings)));

            //services.Configure<LdapPasswordChangeOptions>(Configuration.GetSection(AppSettingsSectionName));
            //services.AddSingleton<IPasswordChangeProvider, LdapPasswordChangeProvider>();
            //services.AddSingleton((ILogger)new LoggerConfiguration()
            //        .MinimumLevel.Information()
            //        .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //            (utcDateTime, wt) => wt.File($"logs/PASSCORE_LDAP_PROVIDER-log-{utcDateTime}.txt"))
            //        .CreateLogger());

            services.Configure<PasswordChangeOptions>(Configuration.GetSection(AppSettingsSectionName));
            services.AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>();
            services.AddSingleton((ILogger)new LoggerConfiguration()
                                .MinimumLevel.Verbose()
                                .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
                                    (utcDateTime, wt) => wt.File($"logs/LDAP_Win-log-{utcDateTime}.txt"))
                                .CreateLogger());

            //.MinimumLevel.Verbose()
            //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            //.Enrich.FromLogContext()
            //.WriteTo.File(Configuration.GetValue<string>("logs/") + "-{Date}.txt", Serilog.Events.LogEventLevel.Information,
            //    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}")
            //.CreateLogger());

            //#if DEBUG
            //            services.Configure<IAppSettings>(Configuration.GetSection(AppSettingsSectionName));
            //            services.AddSingleton<IPasswordChangeProvider, DebugPasswordChangeProvider>();
            //            services.AddSingleton((ILogger)new LoggerConfiguration()
            //                    .MinimumLevel.Information()
            //                    .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //                        (utcDateTime, wt) => wt.File($"logs/DEBUG-log-{utcDateTime}.txt"))
            //                    .CreateLogger());
            //#elif PASSCORE_LDAP_PROVIDER
            //            services.Configure<LdapPasswordChangeOptions>(Configuration.GetSection(AppSettingsSectionName));
            //            services.AddSingleton<IPasswordChangeProvider, LdapPasswordChangeProvider>();
            //            services.AddSingleton((ILogger)new LoggerConfiguration()
            //                    .MinimumLevel.Information()
            //                    .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //                        (utcDateTime, wt) => wt.File($"logs/PASSCORE_LDAP_PROVIDER-log-{utcDateTime}.txt"))
            //                    .CreateLogger());
            //#else
            //            services.Configure<PasswordChangeOptions>(Configuration.GetSection(AppSettingsSectionName));
            //            services.AddSingleton<IPasswordChangeProvider, PasswordChangeProvider>();
            //            services.AddSingleton((ILogger)new LoggerConfiguration()
            //                .MinimumLevel.Information()
            //                .WriteTo.Map("UtcDateTime", DateTime.UtcNow.ToString("yyyyMMdd"),
            //                    (utcDateTime, wt) => wt.File($"logs/LDAP_Win-log-{utcDateTime}.txt"))
            //                .CreateLogger());
            //#endif

            // Add framework services.
            services.AddControllers();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// All arguments are provided through dependency injection.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="settings">The settings.</param>
        public void Configure(IApplicationBuilder app, IOptions<WebSettings> settings)
        {
            if (settings.Value.EnableHttpsRedirect)
                app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            //BaoNX: Enable Cross-Origin Requests (CORS) in ASP.NET Core
            //app.UseCors(MyAllowSpecificOrigins);
            app.UseCors(options => options.AllowAnyOrigin());
            //End Enable Cross-Origin Requests (CORS) in ASP.NET Core

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
