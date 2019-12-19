using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpClientFactoryConsoleSample
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient<IMyService, MyService>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            using var serviceScope = host.Services.CreateScope();
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var myService = services.GetRequiredService<IMyService>();
                    var pageContent = await myService.GetPage().ConfigureAwait(false);

                    Console.WriteLine(pageContent.Substring(0, 500));
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred.");
                }
            }

            return 0;
        }

        public interface IMyService
        {
            Task<string> GetPage();
        }

        public class MyService : IMyService
        {
            public HttpClient HttpClient { get; }

            public MyService(HttpClient httpClient)
            {
                HttpClient = httpClient;
            }

            public async Task<string> GetPage()
            {
                var response = await HttpClient.GetAsync("https://www.bbc.co.uk/programmes/b006q2x0").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}
