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
    /// Unit tests for AdminController.
    /// Tests: AdminLogin, AddFlight, UpdateFlight, DeleteFlight, UpdateUser
    /// </summary>
    [TestFixture]
    public class AdminControllerTests
    {
        private Mock<IAdminService> _mockAdminService = null!;
        private AdminController     _controller       = null!;

        [SetUp]
        public void SetUp()
        {
            _mockAdminService = new Mock<IAdminService>();
            _controller       = new AdminController(_mockAdminService.Object);
        }

        // ── ADMIN LOGIN ───────────────────────────────────────────────────────

        [Test]
        public async Task Login_ValidAdminCredentials_Returns200WithToken()
        {
            // Arrange
            var request  = new AdminLoginRequest { Username = "admin", Password = "admin123" };
            var expected = new AuthResponse
            {
                Token   = "eyJhbGci.admin.token",
                Message = "Admin login successful."
            };
            _mockAdminService.Setup(s => s.AdminLoginAsync(request)).ReturnsAsync(expected);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok = (OkObjectResult)result;
            var response = (AuthResponse)ok.Value!;
            Assert.That(response.Message, Is.EqualTo("Admin login successful."));
        }

        [Test]
        public async Task Login_InvalidAdminCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new AdminLoginRequest { Username = "admin", Password = "wrongpass" };
            _mockAdminService
                .Setup(s => s.AdminLoginAsync(request))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid admin credentials."));

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _controller.Login(request));
        }

        // ── ADD FLIGHT ────────────────────────────────────────────────────────

        [Test]
        public async Task AddFlight_ValidRequest_Returns201WithFlight()
        {
            // Arrange
            var request = new FlightCreateRequest
            {
                FlightNumber   = "AI101",
                Origin         = "Mumbai",
                Destination    = "Delhi",
                FlightDate     = DateTime.UtcNow.AddDays(10),
                Fare           = 4500,
                AvailableSeats = 150
            };
            var expected = new FlightResponse
            {
                FlightId       = 1,
                FlightNumber   = "AI101",
                Origin         = "Mumbai",
                Destination    = "Delhi",
                Fare           = 4500,
                AvailableSeats = 150
            };
            _mockAdminService.Setup(s => s.CreateFlightAsync(request)).ReturnsAsync(expected);

            // Act
            var result = await _controller.AddFlight(request);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var obj = (ObjectResult)result;
            Assert.That(obj.StatusCode, Is.EqualTo(201));
            var flight = (FlightResponse)obj.Value!;
            Assert.That(flight.FlightNumber, Is.EqualTo("AI101"));
        }

        // ── DELETE FLIGHT ─────────────────────────────────────────────────────

        [Test]
        public async Task DeleteFlight_ValidId_Returns204NoContent()
        {
            // Arrange
            _mockAdminService.Setup(s => s.DeleteFlightAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteFlight(1);

            // Assert — must return 204 No Content, NOT 200 OK
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var noContent = (NoContentResult)result;
            Assert.That(noContent.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task DeleteFlight_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockAdminService
                .Setup(s => s.DeleteFlightAsync(999))
                .ThrowsAsync(new KeyNotFoundException("Flight with ID 999 not found."));

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _controller.DeleteFlight(999));
        }

        [Test]
        public async Task DeleteFlight_HasBookings_ThrowsInvalidOperationException()
        {
            // Arrange — flight has bookings, cannot delete
            _mockAdminService
                .Setup(s => s.DeleteFlightAsync(5))
                .ThrowsAsync(new InvalidOperationException("Cannot delete a flight that has existing bookings."));

            // Act & Assert — middleware maps this to 409 Conflict
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _controller.DeleteFlight(5));
        }

        // ── UPDATE USER ───────────────────────────────────────────────────────

        [Test]
        public async Task UpdateUser_ValidRequest_Returns200()
        {
            // Arrange
            var request = new UserUpdateRequest
            {
                UserId   = 1,
                Username = "john_updated",
                Email    = "john_updated@gmail.com",
                Password = "NewPass@123"
            };
            var expected = new SuccessResponse { Message = "User updated successfully." };
            _mockAdminService.Setup(s => s.UpdateUserAsync(request)).ReturnsAsync(expected);

            // Act
            var result = await _controller.UpdateUser(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok = (OkObjectResult)result;
            var response = (SuccessResponse)ok.Value!;
            Assert.That(response.Message, Is.EqualTo("User updated successfully."));
        }
    }
}
