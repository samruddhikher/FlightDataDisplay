using System.Text.Json.Serialization;

namespace FlightDataDisplay.Infrastructure
{
    public class Airport
    {
        [JsonPropertyName("icao")]
        public string Icao { get; set; }

        [JsonPropertyName("iata")]
        public string Iata { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("elevation")]
        public int Elevation { get; set; }

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("tz")]
        public string Timezone { get; set; }

        // Add other fields as needed
    }

}