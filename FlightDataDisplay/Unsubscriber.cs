using System;
using System.Collections.Generic;
using FlightDataDisplay.Application;

namespace FlightDataDisplay.Application
{
    public class Unsubscriber<BaggageInfo> : IDisposable
    {
        private readonly ISet<IObserver<BaggageInfo>> _observers;
        private readonly IObserver<BaggageInfo> _observer;

        public Unsubscriber(ISet<IObserver<BaggageInfo>> observers,IObserver<BaggageInfo> observer)
        {
            _observers = observers;
            _observer = observer;

        }
        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }
}
