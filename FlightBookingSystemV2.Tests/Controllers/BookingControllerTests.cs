using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using FlightBookingSystem.Controllers;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Tests.Controllers
{
    /// <summary>
    /// Unit tests for BookingController.
    /// Tests: Search (valid, not found)
    ///        Book   (success, flight not found, no seats, invalid token)
    /// </summary>
    [TestFixture]
    public class BookingControllerTests
    {
        private Mock<IBookingService> _mockBookingService = null!;
        private BookingController     _controller         = null!;

        [SetUp]
        public void SetUp()
        {
            _mockBookingService = new Mock<IBookingService>();
            _controller         = new BookingController(_mockBookingService.Object);

            // Simulate authenticated customer with ID = 1
            SetAuthenticatedUser(customerId: 1, username: "john");
        }

        private void SetAuthenticatedUser(int customerId, string username)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customerId.ToString()),
                new(ClaimTypes.Name, username)
            };
            var identity  = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // ── SEARCH ────────────────────────────────────────────────────────────

        [Test]
        public async Task Search_ValidReference_Returns200WithBooking()
        {
            // Arrange
            var expected = new BookingResponse
            {
                BookingId       = 1,
                ReferenceNumber = "BK20251201ABCDEF",
                FlightNumber    = "AI101",
                FirstName       = "John",
                LastName        = "Doe",
                BookingStatus   = "Confirmed",
                BaseFare        = 4500,
                FinalFare       = 5310
            };
            _mockBookingService
                .Setup(s => s.GetBookingByReferenceAsync("BK20251201ABCDEF"))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Search(
                new BookingSearchRequest { ReferenceNumber = "BK20251201ABCDEF" });

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok      = (OkObjectResult)result;
            var booking = (BookingResponse)ok.Value!;
            Assert.That(booking.ReferenceNumber, Is.EqualTo("BK20251201ABCDEF"));
            Assert.That(booking.BookingStatus,   Is.EqualTo("Confirmed"));
            Assert.That(booking.FinalFare,       Is.EqualTo(5310));
        }

        [Test]
        public async Task Search_InvalidReference_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockBookingService
                .Setup(s => s.GetBookingByReferenceAsync("INVALID"))
                .ThrowsAsync(new KeyNotFoundException("Booking 'INVALID' not found."));

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _controller.Search(
                    new BookingSearchRequest { ReferenceNumber = "INVALID" }));
        }

        // ── BOOK ──────────────────────────────────────────────────────────────

        [Test]
        public async Task Book_ValidRequest_Returns201WithBooking()
        {
            // Arrange
            var request = new BookingCreateRequest
            {
                FlightId  = 1,
                FirstName = "Jane",
                LastName  = "Smith",
                Gender    = "Female"
            };
            var expected = new BookingResponse
            {
                BookingId       = 2,
                ReferenceNumber = "BK20251202XYZXYZ",
                FlightId        = 1,
                FlightNumber    = "AI101",
                FirstName       = "Jane",
                LastName        = "Smith",
                BookingStatus   = "Confirmed",
                BaseFare        = 4500,
                FinalFare       = 5310,
                IsCheckedIn     = false
            };
            _mockBookingService
                .Setup(s => s.CreateBookingAsync(request, 1))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Book(request);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var obj = (ObjectResult)result;
            Assert.That(obj.StatusCode, Is.EqualTo(201));
            var booking = (BookingResponse)obj.Value!;
            Assert.That(booking.FirstName,     Is.EqualTo("Jane"));
            Assert.That(booking.IsCheckedIn,   Is.False);
            Assert.That(booking.SeatNumber,    Is.Null); // not yet checked in
        }

        [Test]
        public async Task Book_FlightNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new BookingCreateRequest
            {
                FlightId  = 999,
                FirstName = "John",
                LastName  = "Doe",
                Gender    = "Male"
            };
            _mockBookingService
                .Setup(s => s.CreateBookingAsync(request, 1))
                .ThrowsAsync(new KeyNotFoundException("Flight with ID 999 not found."));

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _controller.Book(request));
        }

        [Test]
        public async Task Book_NoSeatsAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new BookingCreateRequest
            {
                FlightId  = 1,
                FirstName = "John",
                LastName  = "Doe",
                Gender    = "Male"
            };
            _mockBookingService
                .Setup(s => s.CreateBookingAsync(request, 1))
                .ThrowsAsync(new InvalidOperationException("No available seats on this flight."));

            // Act & Assert — middleware maps to 409 Conflict
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _controller.Book(request));
        }
    }
}
