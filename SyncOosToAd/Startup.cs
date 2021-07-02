using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SyncOosToAd
{
    public class Startup
    {
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
            services.AddSingleton<IMyService, MyService>();
        }
    }

    public class MyService : IMyService
    {
        private readonly string _baseUrl;
        private readonly string _token;
        private readonly ILogger<MyService> _logger;

        public MyService(ILoggerFactory loggerFactory, IConfigurationRoot config)
        {
            var baseUrl = config["SomeConfigItem:BaseUrl"];
            var token = config["SomeConfigItem:Token"];

            _baseUrl = baseUrl;
            _token = token;
            _logger = loggerFactory.CreateLogger<MyService>();
        }

        public async Task MyServiceMethod()
        {
            _logger.LogDebug(_baseUrl);
            _logger.LogDebug(_token);
        }
    }

    public interface IMyService
    {
        Task MyServiceMethod();
    }
}