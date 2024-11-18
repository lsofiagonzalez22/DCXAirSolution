using DCXAir.Application.Interfaces;
using DCXAir.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace DCXAir.Application.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _repository;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<FlightService> _logger;

        public FlightService(IFlightRepository repository, IConnectionMultiplexer redis, ILogger<FlightService> logger)
        {
            _repository = repository;
            _redis = redis;
            _logger = logger;
        }

        public async Task<List<Journey>> SearchFlightsAsync(string origin, string destination, string currency, string type)
        {
            try
            {
                var cacheKey = $"flights:{origin}:{destination}:{currency}:{type}";
                var db = _redis.GetDatabase();

                var cachedFlights = db.StringGet(cacheKey);
                if (!cachedFlights.IsNullOrEmpty)
                {
                    _logger.LogInformation("Vuelos obtenidos desde la caché para la clave: {CacheKey}", cacheKey);
                    return JsonSerializer.Deserialize<List<Journey>>(cachedFlights);
                }

                _logger.LogInformation("Consultando vuelos desde el repositorio...");
                var flights = await _repository.GetRoutesAsync();
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

                _logger.LogInformation("Guardando resultados en caché para la clave: {CacheKey}", cacheKey);
                db.StringSet(cacheKey, JsonSerializer.Serialize(result), TimeSpan.FromHours(24));

                return result;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error al realizar la conversión de moneda.");
                throw new ApplicationException("Error al realizar la conversión de moneda.", ex);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Error al conectar con el servicio de caché (Redis).");
                throw new ApplicationException("Error al conectar con el servicio de caché (Redis).", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error inesperado al buscar vuelos.");
                throw new ApplicationException("Ocurrió un error inesperado al buscar vuelos.", ex);
            }
        }

        private List<Journey> GetOneWayJourneys(List<Flight> flights, string currency)
        {
            try
            {
                return flights.Select(f => new Journey
                {
                    Flights = new List<Flight> { f },
                    Origin = f.Origin,
                    Destination = f.Destination,
                    Price = ConvertCurrency(f.Price, currency)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar los viajes de ida.");
                throw new ApplicationException("Error al generar los viajes de ida.", ex);
            }
        }

        private List<Journey> GetRoundTripJourneys(List<Flight> flights, List<Flight> filteredFlights, string currency)
        {
            try
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
                            Price = ConvertCurrency(outbound.Price + returnFlight.Price, currency)
                        });
                    }
                }

                return roundTrips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar los viajes de ida y vuelta.");
                throw new ApplicationException("Error al generar los viajes de ida y vuelta.", ex);
            }
        }

        private double ConvertCurrency(double price, string toCurrency)
        {
            try
            {
                var exchangeRates = new Dictionary<string, double>
                {
                    { "USD", 1.0 },
                    { "EUR", 0.92 },
                    { "COP", 4200.0 }
                };

                if (!exchangeRates.ContainsKey(toCurrency))
                {
                    throw new ArgumentException("Moneda no soportada");
                }

                return toCurrency == "USD" ? price : price * exchangeRates[toCurrency];
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error al realizar la conversión de moneda.");
                throw new ApplicationException("Error al realizar la conversión de moneda.", ex);
            }
        }
    }
}
