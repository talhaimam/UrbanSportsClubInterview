using InterviewService.CustomLog;
using InterviewService.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace InterviewService
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("**************************************");
                Console.WriteLine("*  Welcome to the Interview Service  *");
                Console.WriteLine("*************************************");
                Console.WriteLine("");
                Console.WriteLine("Creating WebHost. You should see the configuration of the logging in the next lines..");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webbuilder =>
                {
                    webbuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    // appsettings.json is loaded by default when using CreateDefaultBuilder
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2#json-configuration-provider-1
                    // No need to load it with AddJsonFile.

                    Console.WriteLine($"Production mode: {builderContext.HostingEnvironment.IsProduction()}");

                    config.AddEnvironmentVariables();
                })
                .UseSerilog((x, loggerConfig) =>
                {
                    var config = x.Configuration.Get<Configuration>();

                    var minLevel = config.LOG_LEVEL.ToSerilogEventLevel().Value;
                    loggerConfig.MinimumLevel.Is(minLevel);
                    Console.WriteLine($"[Logging] The minimum logging level is set to {minLevel}");

                    if (config.LOG_LEVEL_EXTRA != null && config.LOG_LEVEL_EXTRA.Value != LogLevel.None)
                    {
                        var extraLevel = config.LOG_LEVEL_EXTRA.Value.ToSerilogEventLevel().Value;
                        loggerConfig.MinimumLevel.Override("Microsoft", extraLevel);
                        loggerConfig.MinimumLevel.Override("System", extraLevel);
                        Console.WriteLine($"[Logging] The extra logging level is set to {extraLevel}");
                    }
                    else
                    {
                        Console.WriteLine("[Logging] Could not read extra logging level from environment variable LOG_LEVEL_EXTRA");
                    }

                    loggerConfig.WriteTo.Sink(new HangfireConsoleSink());
                    loggerConfig.Enrich.FromLogContext();

        #if DEBUG
                    loggerConfig.WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                        theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate
                    );
                    Console.WriteLine("[Logging] Debugging Mode using simple logging pattern");
        #else
                    loggerConfig.WriteTo.Console(formatter: new Serilog.Formatting.Json.JsonFormatter(renderMessage: true));
                    Console.WriteLine("[Logging] Using JsonFormatter for logging");
        #endif

                    //if (config.LOG_SEQ_URL != null)
                    //{
                    //    loggerConfig.WriteTo.Seq(config.LOG_SEQ_URL);
                    //    Console.WriteLine($"[Logging] Seq Sink is enabled and pointing to {config.LOG_SEQ_URL}");
                    //}

                    //if (config.AIRBRAKE_PROJECT_ID != null)
                    //{
                    //    loggerConfig.WriteTo.AirBrakeSink(config.AIRBRAKE_PROJECT_ID, config.AIRBRAKE_PROJECT_KEY, config.ASPNETCORE_ENVIRONMENT);
                    //    Console.WriteLine($"[Logging] Airbrake Sink is enabled");
                    //}

                    Console.WriteLine("[Logging] Configuration finished. From now on logs should come from Serilog");
                });
        }
    }
}
