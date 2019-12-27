using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

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
                IList<Coupon> models;
                var codes = new List<string>{
                        "BOOMMAC65CHRSTMSGME",
                        "BOOMMAC75CHRSTMSGME",
                        "BOOMMAC70CHRSTMSGME"
                    };
                Console.WriteLine($"Start with {DateTime.Now.ToShortTimeString()}: ======>");

                while (true)
                {
                    try
                    {
                        var myService = services.GetRequiredService<IMyService>();
                        models = await myService.GetPage().ConfigureAwait(false);
                        foreach (var coupon in models.Select(x => x.CouponCode))//.Where(x => codes.Select(code => code).Contains(x.CouponCode)))
                        {
                            if (codes.Contains(coupon))
                                continue;
                            Console.WriteLine(coupon);
                            return 0;
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }
        }
        public interface IMyService
        {
            Task<IList<Coupon>> GetPage();
        }

        public class MyService : IMyService
        {
            public HttpClient HttpClient { get; }

            public MyService(HttpClient httpClient)
            {
                HttpClient = httpClient;
            }

            public async Task<IList<Coupon>> GetPage()
            {
                var json = JsonConvert.SerializeObject(new Happy());
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await HttpClient.PostAsync("https://api.globaldelight.net/offer/getoffer/", content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<IList<Coupon>>(result);
            }
        }

        public class Happy
        {
            [JsonProperty("key")]
            public string Key { get; } = "BoomMac";
        }

        public class Coupon
        {
            [JsonProperty("product")]
            public string Product { get; set; }

            [JsonProperty("offer")]
            public long Offer { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("count")]
            public long Count { get; set; }

            [JsonProperty("used")]
            public string Used { get; set; }

            [JsonProperty("coupon_code")]
            public string CouponCode { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}
