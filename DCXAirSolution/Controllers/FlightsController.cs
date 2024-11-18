using Microsoft.AspNetCore.Mvc;
using DCXAir.Application.Interfaces;
using DCXAir.Domain.Entities;

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
        public IActionResult SearchFlights(
            [FromQuery] string? origin,
            [FromQuery] string? destination,
            [FromQuery] string? currency,
            [FromQuery] string? type)
        {
            if (string.IsNullOrEmpty(origin) && string.IsNullOrEmpty(destination) && string.IsNullOrEmpty(currency) && string.IsNullOrEmpty(type))
            {
                return BadRequest("Debe proporcionar al menos un parámetro de búsqueda.");
            }

            var journeys = _flightService.SearchFlights(origin, destination, currency, type);

            if (journeys == null || journeys.Count == 0)
            {
                return NotFound("No se encontraron vuelos que coincidan con los criterios de búsqueda.");
            }

            return Ok(journeys);
        }
    }
}
