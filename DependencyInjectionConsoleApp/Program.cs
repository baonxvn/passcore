using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DependencyInjectionConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            ExemplifyScoping(host.Services, "Scope 1");
            ExemplifyScoping(host.Services, "Scope 2");

            //return host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                services.AddTransient<ITransientOperation, DefaultOperation>()
                    .AddScoped<IScopedOperation, DefaultOperation>()
                    .AddSingleton<ISingletonOperation, DefaultOperation>()
                    .AddTransient<OperationLogger>());
        }

        static void ExemplifyScoping(IServiceProvider services, string scope)
        {
            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            OperationLogger logger = provider.GetRequiredService<OperationLogger>();
            logger.LogOperations($"{scope}-Call 1 .GetRequiredService<OperationLogger>()");

            Console.WriteLine("...");

            logger = provider.GetRequiredService<OperationLogger>();
            logger.LogOperations($"{scope}-Call 2 .GetRequiredService<OperationLogger>()");

            Console.WriteLine();
        }
    }
}
