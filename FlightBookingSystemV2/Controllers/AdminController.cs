using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>Admin login — returns JWT token with Admin role</summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _adminService.AdminLoginAsync(request));
        }

        /// <summary>Add a new flight — Admin only</summary>
        [HttpPost("flights")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddFlight([FromBody] FlightCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var flight = await _adminService.CreateFlightAsync(request);
            return StatusCode(201, flight);
        }

        /// <summary>Update an existing flight — Admin only</summary>
        [HttpPut("flights")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateFlight([FromBody] FlightUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _adminService.UpdateFlightAsync(request));
        }

        /// <summary>Delete a flight by ID — Admin only</summary>
        [HttpDelete("flights/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            await _adminService.DeleteFlightAsync(id);
            return NoContent(); // 204 — success, no body returned
        }

        /// <summary>Update a customer account — Admin only</summary>
        [HttpPut("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _adminService.UpdateUserAsync(request));
        }
    }
}
