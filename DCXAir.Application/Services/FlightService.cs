using DCXAir.Application.Interfaces;
using DCXAir.Domain.Entities;

namespace DCXAir.Application.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _repository;

        public FlightService(IFlightRepository repository)
        {
            _repository = repository;
        }

        public List<Journey> SearchFlights(string origin, string destination, string currency)
        {
            var flights = _repository.GetRoutes();

            var filteredFlights = flights.Where(f => f.Origin == origin && f.Destination == destination).ToList();

            var journeys = filteredFlights.Select(f => new Journey
            {
                Flights = new List<Flight> { f },
                Origin = f.Origin,
                Destination = f.Destination,
                Price = ConvertCurrency(f.Price, f.Transport.FlightCarrier, currency)
            }).ToList();

            return journeys;
        }

        private double ConvertCurrency(double price, string fromCurrency, string toCurrency)
        {
           
            if (fromCurrency != toCurrency)
            {
                if (toCurrency == "USD")
                {
                    return price * 1.2; 
                }
                else if (toCurrency == "EUR")
                {
                    return price * 1.1; 
                }
            }
            return price; 
        }
    }
}
