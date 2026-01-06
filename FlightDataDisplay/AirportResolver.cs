using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;


namespace FlightDataDisplay.Infrastructure
{
    class AirportResolver : IAirportResolver
    {
        private readonly Dictionary<string, Airport> _icaoLookup = new Dictionary<string, Airport>();
        public AirportResolver()
        {
            _icaoLookup = LoadEmbeddedAirportsJson();
        }
        public Airport GetByIcao(string icaoCode)
        {
            
            if(string.IsNullOrEmpty(icaoCode)) return null;
            else return _icaoLookup.TryGetValue(icaoCode?.ToUpper(), out var airport) ? airport : null;
        }

        /*private List<Airport> LoadAirportsFromJson(string jsonFilePath)
        {
            var json = File.ReadAllText(jsonFilePath);
            return JsonConvert.DeserializeObject<List<Airport>>(json);
        }*/

        private Dictionary<string,Airport> LoadEmbeddedAirportsJson()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Resource name format: Namespace.Folder.FileName
            // e.g., "FlightDataDisplay.Data.airports.json"
            var resourceName = "FlightDataDisplay.airports.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new FileNotFoundException(
                    $"Embedded resource '{resourceName}' not found. " +
                    $"Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
            }

            /*using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();*/

            return JsonSerializer.Deserialize<Dictionary<string, Airport>>(stream);
        }
    }
}