using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Produces("application/json")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>Search a booking by reference number</summary>
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Search([FromBody] BookingSearchRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _bookingService.GetBookingByReferenceAsync(request.ReferenceNumber));
        }

        /// <summary>Create a new booking — requires JWT token</summary>
        [HttpPost("book")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Book([FromBody] BookingCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new { message = "Invalid token." });

            var booking = await _bookingService.CreateBookingAsync(request, customerId);
            return StatusCode(201, booking);
        }
    }
}
