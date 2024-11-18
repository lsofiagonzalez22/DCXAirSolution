using DCXAir.Application.Interfaces;
using DCXAir.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;

namespace DCXAir.Infrastructure.Services
{
    public class FlightRepository : IFlightRepository
    {
        private readonly string _filePath = "markets.json";

        private readonly ILogger<FlightRepository> _logger;
        public FlightRepository(ILogger<FlightRepository> logger)
        {
            _logger = logger;
        }

        public async Task<List<Flight>> GetRoutesAsync()
        {
            if (!File.Exists(_filePath))
            {
                _logger.LogError("El archivo {FilePath} no fue encontrado.", _filePath);
                throw new FileNotFoundException($"El archivo {_filePath} no fue encontrado.");
            }

            try
            {
                _logger.LogInformation("Leyendo el archivo de rutas: {FilePath}", _filePath);
                var jsonData = File.ReadAllText(_filePath);

                var flights = JsonSerializer.Deserialize<List<Flight>>(jsonData);
                if (flights == null || flights.Count == 0)
                {
                    _logger.LogWarning("No se encontraron vuelos en el archivo {FilePath}.", _filePath);
                }

                return flights ?? new List<Flight>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Hubo un error al procesar el archivo JSON {FilePath}.", _filePath);
                throw new ApplicationException("Hubo un error al procesar el archivo JSON.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "No se tienen permisos para acceder al archivo {FilePath}.", _filePath);
                throw new ApplicationException("No se tienen permisos para acceder al archivo.", ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Ocurrió un error al leer el archivo {FilePath}.", _filePath);
                throw new ApplicationException("Ocurrió un error al leer el archivo.", ex);
            }
        }
    }
}
