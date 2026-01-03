using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlightDataDisplay.Domain;
using OpenSky;
namespace FlightDataDisplay.Infrastructure
{
    class OpenskyFlightData : IFlightDataRepository
    {
        public static OpenSkyClient client = new OpenSkyClient();

        public OpenskyFlightData()
        {

        }
        public async Task<BaggageInfo> GetAllAsync()
        {
            Console.WriteLine($"Tracking Arrivals in Frankfurt ");
            try
            {
                OpenSkyFlight[] flights = await client.GetAirportArrivalsAsync("EDDF", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow, new System.Threading.CancellationToken());

                for (int i = 0; i < flights.Length; i++)
                {
                    Console.WriteLine($"{flights[i].CallSign} :from {flights[i].EstDepartureAirport} arriving at {flights[i].LastSeen}");
                }
            }
            catch (OpenSkyException e)
            {
                Console.Error.WriteLine(e.Message);
            }

            return new BaggageInfo
            {
                flight = Faker.Name.First().ToUpper().Substring(0, 2).ToString() + " " + Faker.RandomNumber.Next(500, 5000).ToString(),
                from = Faker.Address.City(),
                //flight = flights.FirstOrDefault().CallSign,
                //from = flights.FirstOrDefault().EstDepartureAirport,
                carousel = Faker.RandomNumber.Next(1, 15)
            };
            //throw new System.NotImplementedException();
        }
    }

}
