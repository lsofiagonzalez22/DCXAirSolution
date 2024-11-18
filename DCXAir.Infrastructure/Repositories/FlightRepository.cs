using DCXAir.Application.Interfaces;
using DCXAir.Domain.Entities;
using System.Text.Json;

namespace DCXAir.Infrastructure.Services;

public class FlightRepository : IFlightRepository
{
    private readonly string _filePath = "markets.json";

    public List<Flight> GetRoutes()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"El archivo {_filePath} no fue encontrado.");
        }

        var jsonData = File.ReadAllText(_filePath);
        var flights = JsonSerializer.Deserialize<List<Flight>>(jsonData);

        return flights ?? new List<Flight>();
    }
}
