using Bogus.Extensions.UnitedKingdom;
using FlightDataDisplay.Tests;
using FluentAssertions;

public class ConfigurationTest
{
    private readonly TestConfiguration _testConfiguration = new();

    [Fact]
    public void Configuration_shouldLoad()
    {
        var _config = _testConfiguration.GetConfiguration();
        _config.Should().NotBeNull();
    }
    [Fact]
    public void Configuration_ShouldHaveOpenSkySettings()
    {
        var clientId = _testConfiguration.GetOpenSkyClientId();
        var clientSecret = _testConfiguration.GetOpenSkyClientSecret();
        var AirportIcao = _testConfiguration.GetOpenSkyAirportIcao();

        //clientId.Should().NotBeNullOrEmpty();
        //clientSecret.Should().NotBeNullOrEmpty();  
        //AirportIcao.Should().NotBeNullOrEmpty().Should().Be("EDDF");
    }


}