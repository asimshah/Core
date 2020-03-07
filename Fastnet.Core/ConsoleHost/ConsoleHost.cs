using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Fastnet.Core
{
    /// <summary>
    /// for a console app with DI, create a startup class that implements this
    /// </summary>
    public interface IConsoleStartup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
    /// <summary>
    /// for a console app with DI, create an application class that implements this
    /// </summary>
    public interface IConsoleApp
    {
        /// <summary>
        /// for a console app with DI, call ConsoleHost.Build(args) in Main()
        /// </summary>
        /// <param name="args"></param>
        void Run(string[] args);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    /// <typeparam name="TApp"></typeparam>
    public class ConsoleHost<TStartup, TApp> where TApp : class, IConsoleApp where TStartup : class, IConsoleStartup, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Build(string[] args)
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging((builder) =>
            {
                builder.AddConfiguration(config.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug();
            });
            var ch = new TStartup();
            ch.ConfigureServices(serviceCollection, config);
            var serviceProvider = serviceCollection
            .AddSingleton<IConsoleApp, TApp>()
            .BuildServiceProvider();

            var app = serviceProvider.GetService<IConsoleApp>();
            app.Run(args);
            System.Console.ReadKey();
        }
    }
}
