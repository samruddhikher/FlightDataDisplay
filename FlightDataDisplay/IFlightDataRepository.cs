using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightDataDisplay.Domain
{
    public interface IFlightDataRepository
    {
        Task<BaggageInfo> GetAllAsync();
    }

}