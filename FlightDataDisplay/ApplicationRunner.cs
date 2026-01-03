using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using FlightDataDisplay.Application;
using FlightDataDisplay.Domain;
using Microsoft.Extensions.Hosting;

namespace FlightDataDisplay.Presentation
{
    class ApplicationRunner : IHostedService
    {
        private readonly BaggageHandler _provider;
        private readonly ArrivalsMonitor _observer1;
        private readonly IHostApplicationLifetime _appLifetime;
        //private readonly ArrivalsMonitor _observer2;

        public ApplicationRunner(BaggageHandler provider, ArrivalsMonitor observer1, IHostApplicationLifetime lifetime)
        {
            _provider = provider;
            _observer1 = observer1;
            _appLifetime = lifetime;

            //_observer2 = observer2;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
            //_observer2.Subscribe(_provider);

            /*_provider.BaggageStatus(712, "Detroit", 3);
            Thread.Sleep(1000);*/
            _observer1.Subscribe(_provider);
            /*_provider.BaggageStatus(713, "Kalamazoo", 3);
            Thread.Sleep(1000);
            _provider.BaggageStatus(400, "New York-Kennedy", 1);
            Thread.Sleep(1000);
            _provider.BaggageStatus(712, "Detroit", 3);
            Thread.Sleep(1000);*/


            /*_provider.BaggageStatus(511, "San Francisco", 2);
            Thread.Sleep(1000);
            _provider.BaggageStatus(712);*/
            //_observer2.Unsubscribe();


            /*_provider.BaggageStatus(400);
            _provider.LastBaggageClaimed();*/


            await Task.CompletedTask;
        }

        private void OnStopping()
        {
            Console.WriteLine("Unsubscribing observers");
            _observer1.Unsubscribe();
            //throw new NotImplementedException();
        }

        private void OnStopped()
        {
            //Console.WriteLine("Stopping applicaton");
            //throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _observer1.Unsubscribe();
            return Task.CompletedTask;
            //return Task.FromCanceled(cancellationToken);
        }
    }
}