using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlightDataDisplay.Tests
{
    public class TestConfiguration
    {
        private readonly IConfigurationRoot _configuration;
        public IConfiguration GetConfiguration() => _configuration;
        public string GetOpenSkyClientId() =>_configuration["OpenSky:ClientId"] ?? "test-client-id";

        public string GetOpenSkyClientSecret() =>
            _configuration["OpenSky:ClientSecret"] ?? "test-client-secret";

        public string GetOpenSkyAirportIcao() =>
            _configuration["OpenSky:AirportIcao"] ?? "EDDF";
        public TestConfiguration()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();

            builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.Tests.json", optional: true)
            .AddUserSecrets<TestConfiguration>(optional: true)
            .AddEnvironmentVariables();

            _configuration = builder.Build();
        }
    }

}
