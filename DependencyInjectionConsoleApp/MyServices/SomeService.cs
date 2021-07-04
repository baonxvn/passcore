using System;
using Microsoft.Extensions.Configuration;

namespace DependencyInjectionConsoleApp.MyServices
{
    public class SomeService : ISomeService
    {
        IConfiguration configuration;

        public SomeService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void DoProcess()
        {
            var value = configuration["SomeKey"];
            Console.WriteLine("Value from the Config is: " + value);
        }

        public string TestDi()
        {
            return "This is string";
        }
    }
}