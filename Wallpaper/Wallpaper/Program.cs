using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Wallpaper.Configs;
using Wallpaper.Services;

namespace Wallpaper
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Specifying the configuration for serilog
            var configuration = builder.Build();

            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                .ReadFrom.Configuration(configuration) // connect serilog to our configuration folder
                .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System.Net.Http.*", LogEventLevel.Error)
                .WriteTo.Console() // decide where the logs are going to be shown
                .CreateLogger(); //initialise the logger

            var host = Host.CreateDefaultBuilder() // Initialising the Host
                .ConfigureServices((context, services) => { // Adding the DI container for configuration
                    ConfigureServices(services, configuration);
                })
                .UseSerilog() // Add Serilog
                .Build();

            await Start(host.Services);
            return 0;
        }

        private static async Task Start(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Application Started");

                var sw = Stopwatch.StartNew();

                await services.GetRequiredService<Startup>().Run(default);

                sw.Stop();
                logger.LogInformation("Application finished ({@Elapsed})", sw.Elapsed);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Fatal error");
            }
            finally
            {
                Console.WriteLine("Enter for exit console..");
                Console.ReadLine();
            }
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<Startup>();
            ConfigureAndValidate<WallPaperConfig>(services, configuration.GetSection(WallPaperConfig.SectionName));

            services.AddHttpClient<YandexService>();
            services.AddScoped<WallPaperGenerator>();

            // Place the following after all AddHttpClient registrations.
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
        }

        private static void ConfigureAndValidate<T>(IServiceCollection services, IConfiguration section)
            where T : class, IValidateOptions<T>
        {
            services.Configure<T>(section);

            services.AddSingleton<IValidateOptions<T>, T>();
        }
    }
}


