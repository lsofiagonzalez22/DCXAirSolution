using DCXAir.Application.Interfaces;
using DCXAir.Domain.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace DCXAir.Application.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _repository;
        private readonly IConnectionMultiplexer _redis;

        public FlightService(IFlightRepository repository, IConnectionMultiplexer redis)
        {
            _repository = repository;
            _redis = redis;
        }

        public List<Journey> SearchFlights(string origin, string destination, string currency, string type)
        {

            var cacheKey = $"flights:{origin}:{destination}:{currency}:{type}";
            var db = _redis.GetDatabase();

            var cachedFlights = db.StringGet(cacheKey);
            if (!cachedFlights.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<List<Journey>>(cachedFlights);
            }
            var flights = _repository.GetRoutes();
            List<Flight> filteredFlights = flights;

            if (!string.IsNullOrEmpty(origin))
                 filteredFlights = filteredFlights.Where(f => f.Origin == origin).ToList();

            if (!string.IsNullOrEmpty(destination))
                filteredFlights = filteredFlights.Where(f => f.Destination == destination).ToList();
            List<Journey> result;

            if (type?.ToLower() == "oneway")
            {
                result = GetOneWayJourneys(filteredFlights, currency);
            }
            else if (type?.ToLower() == "roundtrip")
            {
                result = GetRoundTripJourneys(flights, filteredFlights, currency);
            }
            else
            {
                var oneWayJourneys = GetOneWayJourneys(filteredFlights, currency);
                var roundTripJourneys = GetRoundTripJourneys(flights, filteredFlights, currency);
                result = oneWayJourneys.Concat(roundTripJourneys).ToList();
            }

            db.StringSet(cacheKey, JsonSerializer.Serialize(result), TimeSpan.FromHours(24));

            return result;
        }
        private List<Journey> GetOneWayJourneys(List<Flight> flights, string currency)
        {
            return flights.Select(f => new Journey
            {
                Flights = new List<Flight> { f },
                Origin = f.Origin,
                Destination = f.Destination,
                Price = ConvertCurrency(f.Price, f.Transport.FlightCarrier, currency)
            }).ToList();
        }

        private List<Journey> GetRoundTripJourneys(List<Flight> flights, List<Flight> filteredFlights, string currency)
        {
            var roundTrips = new List<Journey>();

            foreach (var outbound in filteredFlights)
            {
                var returnFlights = flights.Where(f => f.Origin == outbound.Destination && f.Destination == outbound.Origin).ToList();

                foreach (var returnFlight in returnFlights)
                {
                    roundTrips.Add(new Journey
                    {
                        Flights = new List<Flight> { outbound, returnFlight },
                        Origin = outbound.Origin,
                        Destination = outbound.Destination,
                        Price = ConvertCurrency(outbound.Price + returnFlight.Price, outbound.Transport.FlightCarrier, currency)
                    });
                }
            }

            return roundTrips;
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
