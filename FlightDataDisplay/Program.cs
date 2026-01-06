using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FlightDataDisplay.Application;
using FlightDataDisplay.Domain;
using FlightDataDisplay.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;

namespace FlightDataDisplay.Presentation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context,config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",optional:true,reloadOnChange:true)
                .AddJsonFile($"appsettings{context.HostingEnvironment.EnvironmentName}.json",optional:true);
                if(context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }

                config.AddEnvironmentVariables(prefix: "FLIGHTDATADISPLAY_");

                config.AddCommandLine(args);
            })

     
            .ConfigureServices((context, services) =>
            {
                IConfiguration configuration = context.Configuration;
                Console.WriteLine("secrets" + configuration["OpenSky:ClientId"] + configuration["OpenSky:ClientSecret"]);
                services.AddSingleton<BaggageHandler>();
                services.AddSingleton<ArrivalsMonitor>(name => new ArrivalsMonitor("Security Exit"));
                services.AddSingleton<IAirportResolver, AirportResolver>();
                services.AddHttpClient("OpenSky", client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
                //services.AddSingleton<ArrivalsMonitor>(name=> new ArrivalsMonitor("BaggageClaimMonitor"));
                services.AddSingleton<IFlightDataRepository>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var airportResolver = sp.GetRequiredService<IAirportResolver>();

                    var clientId = configuration["OpenSky:ClientId"];
                    var clientSecret = configuration["OpenSky:ClientSecret"];
                    var airportIcao = configuration["OpenSky:AirportIcao"] ?? "EDDF";

                    Console.WriteLine(clientId, clientSecret);

                    return new OpenskyFlightData(httpClientFactory, airportResolver, clientId, clientSecret, airportIcao);
                })
               .BuildServiceProvider();

                services.AddHostedService<ApplicationRunner>();

            }).Build();

            await host.RunAsync();


            /*BaggageHandler provider = new BaggageHandler();
            ArrivalsMonitor observer1 = new ArrivalsMonitor("BaggageClaimMonitor1");
            ArrivalsMonitor observer2 = new("SecurityExit");
            observer2.Subscribe(provider);

            provider.BaggageStatus(712, "Detroit", 3);
            Thread.Sleep(1000);
            observer1.Subscribe(provider);  
            provider.BaggageStatus(713, "Kalamazoo", 3);
            Thread.Sleep(1000);
            provider.BaggageStatus(400, "New York-Kennedy", 1);
            Thread.Sleep(1000);
            provider.BaggageStatus(712, "Detroit", 3);
            Thread.Sleep(1000);
            

            provider.BaggageStatus(511, "San Francisco", 2);
            Thread.Sleep(1000);
            provider.BaggageStatus(712);
            observer2.Unsubscribe();
            

            provider.BaggageStatus(400);
            provider.LastBaggageClaimed();
            observer1.Unsubscribe();*/

        }
    }
}
