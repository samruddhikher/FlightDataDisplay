using System;
using System.Collections.Generic;
using FlightDataDisplay.Domain;
using FlightDataDisplay.Application;

namespace FlightDataDisplay.Presentation
{
    public class ArrivalsMonitor : IObserver<BaggageInfo>
    {
        private readonly string _name;
        private readonly List<string> _flights = new List<string>();
        private readonly string _format = "{0,-20} {1,5}  {2, 3}";
        private IDisposable? _cancellation;

        public ArrivalsMonitor(string name)
        {
            if (name == null) throw new ArgumentNullException();
            _name = name;
        }

        public virtual void Subscribe(BaggageHandler provider) 
        {
           _cancellation =  provider.Subscribe(this);
        }
        public virtual void Unsubscribe()
        {
            _cancellation?.Dispose();
            _flights.Clear();
        }

        public virtual void OnCompleted() => _flights.Clear();

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(BaggageInfo info)
        {
            bool updated = false;

            if (info.carousel is 0)
            {
                string flightNumber = string.Format("{0,6}", info.flight);
                for (int index = _flights.Count - 1; index >= 0; index--)
                {
                    string flightInfo = _flights[index];
                    if (flightInfo.Substring(21, 5).Trim().Equals(flightNumber))
                    {
                        updated = true;
                        _flights.RemoveAt(index);
                    }
                }

            }
            else
            {
                // Add flight if it doesn't exist in the collection.
                string flightInfo = string.Format(_format, info.from, info.flight, info.carousel);
                if (_flights.Contains(flightInfo) is false)
                {
                    _flights.Add(flightInfo);
                    updated = true;
                }
            }
            if (updated)
            {
                _flights.Sort();
                Console.WriteLine($"Arrivals information from {_name}");
                foreach (string flightInfo in _flights)
                {
                    Console.WriteLine(flightInfo);
                }
                Console.WriteLine();
            }

        }
    }
}
