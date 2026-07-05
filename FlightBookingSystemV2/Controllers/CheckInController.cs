using Microsoft.AspNetCore.Mvc;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Controllers
{
    [ApiController]
    [Route("api/checkin")]
    [Produces("application/json")]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        /// <summary>Search a booking before check-in</summary>
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Search([FromBody] CheckInRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _checkInService.SearchBookingAsync(request.ReferenceNumber));
        }

        /// <summary>Perform online check-in — seat auto-assigned</summary>
        [HttpPost("perform")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Perform([FromBody] CheckInRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _checkInService.PerformCheckInAsync(request));
        }

        /// <summary>Get check-in status for a booking reference</summary>
        [HttpGet("status/{referenceNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Status(string referenceNumber)
        {
            return Ok(await _checkInService.GetCheckInStatusAsync(referenceNumber));
        }
    }
}
