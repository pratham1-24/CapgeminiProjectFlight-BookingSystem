using Microsoft.AspNetCore.Mvc;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Register a new customer account</summary>
        [HttpPost("user/register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.RegisterAsync(request);
            return StatusCode(201, result);
        }

        /// <summary>Login as customer and receive JWT token</summary>
        [HttpPost("user/login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _authService.LoginAsync(request));
        }
    }
}
