using Microsoft.AspNetCore.Mvc;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Controllers
{
    [ApiController]
    [Route("api/flights")]
    [Produces("application/json")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        /// <summary>Search available flights by origin, destination and date</summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] string origin,
            [FromQuery] string destination, [FromQuery] DateTime flightDate)
        {
            var request = new FlightSearchRequest
            {
                Origin = origin,
                Destination = destination,
                FlightDate = flightDate
            };
            var flights = await _flightService.SearchFlightsAsync(request);
            return Ok(flights);
        }
    }
}
