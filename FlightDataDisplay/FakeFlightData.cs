using System.Collections.Generic;
using System.Threading.Tasks;
using FlightDataDisplay.Domain;
using Faker;
using System.Timers;
using Microsoft.VisualBasic;
using System;
using System.Linq;

namespace FlightDataDisplay.Application
{
    class FakeFlightData : IFlightDataRepository
    {
        public FakeFlightData()
        {
        }
        public async Task<BaggageInfo> GetAllAsync()
        {
            return await Task.FromResult(GetBaggageInfo());
        }


        private BaggageInfo GetBaggageInfo()
        {
            return new BaggageInfo()
            {
                flight = Faker.Name.First().ToUpper().Substring(0,2).ToString()+ " " + Faker.RandomNumber.Next(500, 5000).ToString(),
                from = Faker.Address.City(),
                carousel = Faker.RandomNumber.Next(0, 5)
            };
        }
    }

}