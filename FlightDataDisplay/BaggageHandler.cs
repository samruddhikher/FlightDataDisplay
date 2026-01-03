using System;
using System.Timers;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightDataDisplay.Domain;
using System.Linq;

namespace FlightDataDisplay.Application
{
    public class BaggageHandler : IObservable<BaggageInfo>
    {
        private readonly IFlightDataRepository _repo;
        private System.Timers.Timer timer;
        private System.Timers.Timer deleteTimer;
        private HashSet<IObserver<BaggageInfo>> _observers = new HashSet<IObserver<BaggageInfo>>();
        private HashSet<BaggageInfo> _flights = new HashSet<BaggageInfo>();
        public BaggageHandler(IFlightDataRepository repo)
        {
            _repo = repo;
            timer = new System.Timers.Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
            timer.Enabled = true;

            deleteTimer = new System.Timers.Timer(TimeSpan.FromSeconds(4).TotalMilliseconds);
            deleteTimer.Enabled = true;
            deleteTimer.Elapsed += RemoveData;
            //timer.Interval = 100 /*Convert.ToDouble(TimeSpan.FromSeconds(2).TotalMilliseconds)*/;
            timer.Elapsed += BaggageStatus;
        }
        public IDisposable Subscribe(IObserver<BaggageInfo> observer)
        {

            if (!_observers.Contains(observer)) _observers.Add(observer);

            foreach (BaggageInfo item in _flights)
            {
                observer.OnNext(item);
            }
            return new Unsubscriber<BaggageInfo>(_observers, observer);
        }

        // Called to indicate all baggage is now unloaded.
        public async Task BaggageStatus(string flightNumber) => await BaggageStatus(flightNumber, string.Empty, 0);

        public async Task BaggageStatus(string flightNumber, string from, int carousel)
        {
            var info = new BaggageInfo()
            {
                flight = flightNumber,
                from = from,
                carousel = carousel
            };
            if (carousel > 0)
            {
                _flights.Add(info);
                foreach (IObserver<BaggageInfo> observer in _observers)
                {
                    observer.OnNext(info);
                }
            }
            else if (carousel == 0)
            {

                _flights.RemoveWhere(x => x.flight == flightNumber);
                foreach (IObserver<BaggageInfo> observer in _observers)
                {
                    observer.OnNext(info);
                }
            }
        }

        public async void BaggageStatus(object sender, ElapsedEventArgs e)
        {
            BaggageInfo info = await _repo.GetAllAsync();

            if (info.carousel > 0)
            {
                _flights.Add(info);
                foreach (IObserver<BaggageInfo> observer in _observers)
                {
                    observer.OnNext(info);
                }
            }
            else if (info.carousel == 0)
            {

                _flights.RemoveWhere(x => x.flight == info.flight);
                foreach (IObserver<BaggageInfo> observer in _observers)
                {
                    observer.OnNext(info);
                }
            }
        }

        async void RemoveData(object sender, ElapsedEventArgs e)
        {
            Random random = new Random();
            if (_flights.Count > 0)
            {
                BaggageInfo flight = _flights.ElementAt(_flights.Count-1/*random.Next(0, _flights.Count)*/);

                Console.WriteLine($"Removing {flight.flight} from {flight.from}");
                await BaggageStatus(flight.flight);
            }



        }

        public void LastBaggageClaimed()
        {
            foreach (IObserver<BaggageInfo> observer in _observers)
            {
                observer.OnCompleted();
            }

            _observers.Clear();
        }

    }

}

