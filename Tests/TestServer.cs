using InterviewService.Client;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Polly.Timeout;
using System;

namespace InterviewService.Tests
{
    public static class TestServer
    {
        private static readonly object _serviceLock = new object();
        private static bool _isRunning;

        /// <summary>
        /// This starts the service with the configuration from the ./Tests/appsettings.json
        /// Make sure to start it only once per test
        /// </summary>
        /// <exception cref="TimeoutException">Throws if service didnt start</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "<Pending>")]
        public static void StartService()
        {
            lock (_serviceLock)
            {
                if (_isRunning == false)
                {
                    WebHost.CreateDefaultBuilder()
                        .UseKestrel()
                        .UseUrls("http://localhost:5000")
                        .UseStartup<Startup>()
                        .ConfigureAppConfiguration((builderContext, config) =>
                        {
                            config.AddJsonFile("appsettings.json", false);
                        })
                        .Build()
                        .RunAsync();

                    InterviewServiceClient client = new InterviewServiceClient("http://localhost:5000");

                    _isRunning = TimeoutPolicy.TimeoutAsync(60)
                        .ExecuteAsync(() => client.CheckHealth())
                        .GetAwaiter().GetResult();

                    if (_isRunning == false)
                    {
                        throw new TimeoutException("The service didnt start yet");
                    }
                }
            }
        }
    }
}
