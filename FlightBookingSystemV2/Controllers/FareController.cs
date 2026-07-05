using Microsoft.AspNetCore.Mvc;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Controllers
{
    [ApiController]
    [Route("api/fare")]
    [Produces("application/json")]
    public class FareController : ControllerBase
    {
        private readonly IFareService _fareService;

        public FareController(IFareService fareService)
        {
            _fareService = fareService;
        }

        /// <summary>Get fare details by flight number (includes GST breakdown)</summary>
        [HttpGet("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFareByFlightNumber([FromQuery] string flightNumber)
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
                return BadRequest(new { message = "Flight number is required." });

            return Ok(await _fareService.GetFareByFlightNumberAsync(flightNumber));
        }

        /// <summary>Get fare details by flight ID (includes GST breakdown)</summary>
        [HttpGet("{flightId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFareByFlightId(int flightId)
        {
            return Ok(await _fareService.GetFareByFlightIdAsync(flightId));
        }
    }
}
