using DCXAir.Application.Interfaces;
using DCXAir.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DCXAir.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        /// <summary>
        /// Endpoint to search for flights based on the filters provided by user.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights(
            [FromQuery] string? origin,
            [FromQuery] string? destination,
            [FromQuery] string? currency,
            [FromQuery] string? type)
        {
            if (string.IsNullOrEmpty(origin) && string.IsNullOrEmpty(destination) && string.IsNullOrEmpty(currency) && string.IsNullOrEmpty(type))
            {
                return BadRequest("Debe proporcionar al menos un parámetro de búsqueda.");
            }

            try
            {
                var journeys = await _flightService.SearchFlightsAsync(origin, destination, currency, type);

                return Ok(journeys ?? new List<Journey>());
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Error en los parámetros de búsqueda: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error inesperado: {ex.Message}");
            }
        }
    }
}
