using System.Threading;
using System.Threading.Tasks;
using FlightDataDisplay.Application;
using FlightDataDisplay.Domain;
using FlightDataDisplay.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlightDataDisplay.Presentation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
               services.AddSingleton<BaggageHandler>();
               services.AddSingleton<ArrivalsMonitor>(name=> new ArrivalsMonitor("Security Exit"));
               //services.AddSingleton<ArrivalsMonitor>(name=> new ArrivalsMonitor("BaggageClaimMonitor"));
               services.AddScoped<IFlightDataRepository, OpenskyFlightData>()
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
