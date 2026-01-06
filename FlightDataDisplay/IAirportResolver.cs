namespace FlightDataDisplay.Infrastructure
{
    public interface IAirportResolver
    {
        Airport GetByIcao(string icaoCode);
    }
    
}