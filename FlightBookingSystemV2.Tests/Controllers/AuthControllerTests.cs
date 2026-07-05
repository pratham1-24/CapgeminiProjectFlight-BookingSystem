using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using FlightBookingSystem.Controllers;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Tests.Controllers
{
    /// <summary>
    /// Unit tests for AuthController.
    /// Tests: Register (success, duplicate username, duplicate email)
    ///        Login   (success, invalid credentials)
    /// </summary>
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _mockAuthService = null!;
        private AuthController     _controller      = null!;

        [SetUp]
        public void SetUp()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller      = new AuthController(_mockAuthService.Object);
        }

        // ── REGISTER ──────────────────────────────────────────────────────────

        [Test]
        public async Task Register_ValidDetails_Returns201WithToken()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                Username = "john",
                Email    = "john@gmail.com",
                Password = "Pass@123"
            };
            var expected = new AuthResponse
            {
                Token     = "eyJhbGci.valid.token",
                Message   = "User registered successfully.",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(expected);

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objResult = (ObjectResult)result;
            Assert.That(objResult.StatusCode, Is.EqualTo(201));
            var response = (AuthResponse)objResult.Value!;
            Assert.That(response.Message, Is.EqualTo("User registered successfully."));
            Assert.That(response.Token,   Is.Not.Empty);
        }

        [Test]
        public async Task Register_DuplicateUsername_ThrowsArgumentException()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                Username = "existinguser",
                Email    = "new@gmail.com",
                Password = "Pass@123"
            };
            _mockAuthService
                .Setup(s => s.RegisterAsync(request))
                .ThrowsAsync(new ArgumentException("Username already exists."));

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.Register(request));
        }

        [Test]
        public async Task Register_DuplicateEmail_ThrowsArgumentException()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                Username = "newuser",
                Email    = "existing@gmail.com",
                Password = "Pass@123"
            };
            _mockAuthService
                .Setup(s => s.RegisterAsync(request))
                .ThrowsAsync(new ArgumentException("Email already registered."));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.Register(request));
            Assert.That(ex!.Message, Is.EqualTo("Email already registered."));
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        [Test]
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            // Arrange
            var request = new UserLoginRequest { Username = "john", Password = "Pass@123" };
            var expected = new AuthResponse
            {
                Token   = "eyJhbGci.valid.token",
                Message = "Login successful."
            };
            _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(expected);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok = (OkObjectResult)result;
            var response = (AuthResponse)ok.Value!;
            Assert.That(response.Token,   Is.Not.Empty);
            Assert.That(response.Message, Is.EqualTo("Login successful."));
        }

        [Test]
        public async Task Login_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new UserLoginRequest { Username = "john", Password = "WrongPassword" };
            _mockAuthService
                .Setup(s => s.LoginAsync(request))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _controller.Login(request));
            Assert.That(ex!.Message, Is.EqualTo("Invalid credentials."));
        }
    }
}
