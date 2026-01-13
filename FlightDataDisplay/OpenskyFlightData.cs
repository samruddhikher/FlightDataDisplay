using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using FlightDataDisplay.Domain;
using Newtonsoft.Json;
using System.Collections.Concurrent;
namespace FlightDataDisplay.Infrastructure
{
    class OpenskyFlightData : IFlightDataRepository, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAirportResolver _airportResolver;
        private readonly HttpClient _client;
        private readonly Timer _refreshTimer;
        private const int MinCarousel = 1;
        private const int MaxCarousel = 15;
        private const int RefreshIntervalMinutes = 30;
        private const string OpenSkyAuthUrl = "https://auth.opensky-network.org/auth/realms/opensky-network/protocol/openid-connect/token";
        private const string OpenSkyApiBaseUrl = "https://opensky-network.org/api";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _airportIcao;
        ConcurrentDictionary<string, FlightArrival> _flights;
        List<BaggageInfo> _flightsRecieved = new List<BaggageInfo>();
        private string _accessToken;
        private bool _disposed;
        //public event Action<FlightArrival> OnFlightAdded;
        //public event Action<FlightArrival> OnFlightUpdated;
        public OpenskyFlightData(IHttpClientFactory httpClientFactory, IAirportResolver airportResolver, string clientId,
            string clientSecret,
            string airportIcao = "EDDF")
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _airportResolver = airportResolver ?? throw new ArgumentNullException(nameof(airportResolver));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _airportIcao = airportIcao ?? throw new ArgumentNullException(nameof(airportIcao));
            _client = _httpClientFactory.CreateClient("OpenSky");
            _flights = new ConcurrentDictionary<string, FlightArrival>();
            _refreshTimer = new Timer(TimeSpan.FromMinutes(RefreshIntervalMinutes).TotalMilliseconds)
            {
                AutoReset = true
            };
            _refreshTimer.Elapsed += OnRefreshTimerElapsed;

            // Initial data fetch
            _ = RefreshFlightDataAsync();

            _refreshTimer.Start();
        }

        private async void OnRefreshTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await RefreshFlightDataAsync();
        }
        private async Task RefreshFlightDataAsync()
        {
            try
            {
                await AuthenticateAsync();
                await FetchFlightDataAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error refreshing flight data: {ex.Message}");
            }
        }

        private async Task AuthenticateAsync()
        {
            try
            {
                _accessToken = await GetAccessTokenAsync(_clientId, _clientSecret);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Authentication failed: {ex.Message}");
                throw;
            }
        }
        private async Task FetchFlightDataAsync()
        {
            try
            {
                var url = BuildApiUrl(_airportIcao);
                var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"API request failed with status: {response.StatusCode}");
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                List<FlightArrival> flights = new List<FlightArrival>();
                flights = JsonConvert.DeserializeObject<List<FlightArrival>>(content);
                //Console.WriteLine(content);

                if (flights == null || !flights.Any())
                {
                    Console.WriteLine("No flights received from API");
                    return;
                }
                MergeFlights(flights);
                flights.Clear();

            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Error fetching flight data: {ex.Message}");
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Console.Error.WriteLine($"Error parsing flight data: {ex.Message}");
            }
        }
        private static string BuildApiUrl(string airportIcao)
        {
            var begin = DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds();
            var end = DateTimeOffset.Now.ToUnixTimeSeconds();
            return $"{OpenSkyApiBaseUrl}/flights/arrival?airport={airportIcao}&begin={begin}&end={end}";
        }

        public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret)
        {
            var requestData = new FormUrlEncodedContent(new[]

                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });    // OpenSky OAuth2 token endpoint
            var response = await _client.PostAsync(OpenSkyAuthUrl, requestData);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();


            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }
        public Task<BaggageInfo> GetAllAsync()
        {
            if (_flights == null || _flights.IsEmpty)
            {
                Console.WriteLine("No New/Updated Flight Data");
                return Task.FromResult<BaggageInfo>(null);
            }
            var flight = _flights.Values.OrderByDescending(x=>x.lastSeen).FirstOrDefault();

            var key = GetFlightKey(flight);
            _flights.TryRemove(key, out _);
            var originAirport = _airportResolver.GetByIcao(flight.EstDepartureAirport);


            var baggageInfo = new BaggageInfo
            {
                flight = flight.CallSign ?? "Unknown",
                from = originAirport?.City ?? flight.EstDepartureAirport ?? "Unknown",
                arrival = DateTimeOffset.FromUnixTimeSeconds(flight.lastSeen).DateTime,
                carousel = Random.Shared.Next(MinCarousel, MaxCarousel + 1)
            };

            return Task.FromResult(baggageInfo);
        }
        private void MergeFlights(List<FlightArrival> newFlights)
        {
            int addedCount = 0;
            int updatedCount = 0;
            int unchangedCount = 0;

            foreach (var newFlight in newFlights)
            {

                var key = GetFlightKey(newFlight);
                //Console.WriteLine($"{key} for {newFlight.CallSign}");
                if (_flights.TryGetValue(key, out var existingFlight))
                {
                    if (!AreFlightsEqual(existingFlight, newFlight))
                    {
                        _flights[key] = newFlight;  // Update
                        updatedCount++;

                        //OnFlightUpdated?.Invoke(newFlight);

                        Console.WriteLine($"Updated: {newFlight.CallSign} : " +
                                        $"LastSeen: {existingFlight.lastSeen} → {newFlight.lastSeen}");
                    }
                    else
                    {
                        unchangedCount++;
                    }
                }
                else
                {
                    // New flight
                    _flights[key] = newFlight;
                    addedCount++;
                    //OnFlightAdded?.Invoke(newFlight);

                    Console.WriteLine($"New: {newFlight.CallSign} from {newFlight.EstDepartureAirport}");
                }
            }

            Console.WriteLine($"Flights : {_flights}");
            Console.WriteLine($"Total flights tracked: {_flights.Count}");
            Console.WriteLine($"This update: +{addedCount} ~{updatedCount} ={unchangedCount}");
        }
        private static string GetFlightKey(FlightArrival flight)
        {
            return $"{flight.icao24}_{flight.CallSign}_{flight.firstSeen}";
        }
        private static bool AreFlightsEqual(FlightArrival f1, FlightArrival f2)
        {
            return f1.icao24 == f2.icao24 &&
               f1.firstSeen == f2.firstSeen &&
               f1.lastSeen == f2.lastSeen &&
               f1.EstDepartureAirport == f2.EstDepartureAirport &&
               f1.estArrivalAirport == f2.estArrivalAirport &&
               f1.CallSign == f2.CallSign;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
                // Note: Don't dispose _client if using IHttpClientFactory
            }

            _disposed = true;
        }
    }

    public record FlightArrival(string icao24, long firstSeen, string EstDepartureAirport,
                        long lastSeen, string estArrivalAirport, string CallSign);
}


