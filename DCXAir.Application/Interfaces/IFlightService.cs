using DCXAir.Domain.Entities;

namespace DCXAir.Application.Interfaces
{
    public interface IFlightService
    {
        Task<List<Journey>> SearchFlightsAsync(string origin, string destination, string currency, string type);

    }

}
