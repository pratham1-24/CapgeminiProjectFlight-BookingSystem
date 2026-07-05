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
    /// Unit tests for CheckInController.
    /// Tests: Search  (valid, not found)
    ///        Perform (success, already checked in, booking not found, cancelled)
    ///        Status  (checked in, not yet checked in)
    /// </summary>
    [TestFixture]
    public class CheckInControllerTests
    {
        private Mock<ICheckInService> _mockCheckInService = null!;
        private CheckInController     _controller         = null!;

        [SetUp]
        public void SetUp()
        {
            _mockCheckInService = new Mock<ICheckInService>();
            _controller         = new CheckInController(_mockCheckInService.Object);
        }

        // ── SEARCH ────────────────────────────────────────────────────────────

        [Test]
        public async Task Search_ValidReference_Returns200WithBookingDetails()
        {
            // Arrange
            var expected = new BookingResponse
            {
                BookingId       = 1,
                ReferenceNumber = "BK20251201ABCDEF",
                FlightNumber    = "AI101",
                FirstName       = "John",
                IsCheckedIn     = false,
                BookingStatus   = "Confirmed"
            };
            _mockCheckInService
                .Setup(s => s.SearchBookingAsync("BK20251201ABCDEF"))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Search(
                new CheckInRequest { ReferenceNumber = "BK20251201ABCDEF" });

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok      = (OkObjectResult)result;
            var booking = (BookingResponse)ok.Value!;
            Assert.That(booking.IsCheckedIn, Is.False);
        }

        [Test]
        public async Task Search_BookingNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockCheckInService
                .Setup(s => s.SearchBookingAsync("UNKNOWN"))
                .ThrowsAsync(new KeyNotFoundException("Booking 'UNKNOWN' not found."));

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _controller.Search(
                    new CheckInRequest { ReferenceNumber = "UNKNOWN" }));
        }

        // ── PERFORM ───────────────────────────────────────────────────────────

        [Test]
        public async Task Perform_ValidRequest_Returns200WithSeatAssigned()
        {
            // Arrange
            var expected = new CheckInResponse
            {
                CheckInId        = 1,
                CheckInReference = "CI20251201ABC123",
                BookingReference = "BK20251201ABCDEF",
                SeatNumber       = "14C",
                CheckInStatus    = "Completed",
                PassengerName    = "John Doe",
                FlightNumber     = "AI101",
                Message          = "Check-in successful!"
            };
            _mockCheckInService
                .Setup(s => s.PerformCheckInAsync(It.IsAny<CheckInRequest>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Perform(
                new CheckInRequest { ReferenceNumber = "BK20251201ABCDEF" });

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok       = (OkObjectResult)result;
            var response = (CheckInResponse)ok.Value!;
            Assert.That(response.SeatNumber,    Is.EqualTo("14C"));
            Assert.That(response.CheckInStatus, Is.EqualTo("Completed"));
            Assert.That(response.Message,       Is.EqualTo("Check-in successful!"));
        }

        [Test]
        public async Task Perform_AlreadyCheckedIn_ThrowsArgumentException()
        {
            // Arrange
            _mockCheckInService
                .Setup(s => s.PerformCheckInAsync(It.IsAny<CheckInRequest>()))
                .ThrowsAsync(new ArgumentException("Passenger is already checked in."));

            // Act & Assert — middleware maps to 400 Bad Request
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.Perform(
                    new CheckInRequest { ReferenceNumber = "BK20251201ABCDEF" }));
            Assert.That(ex!.Message, Is.EqualTo("Passenger is already checked in."));
        }

        [Test]
        public async Task Perform_BookingNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockCheckInService
                .Setup(s => s.PerformCheckInAsync(It.IsAny<CheckInRequest>()))
                .ThrowsAsync(new KeyNotFoundException("Booking 'INVALID' not found."));

            // Act & Assert — middleware maps to 404 Not Found
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _controller.Perform(
                    new CheckInRequest { ReferenceNumber = "INVALID" }));
        }

        [Test]
        public async Task Perform_CancelledBooking_ThrowsArgumentException()
        {
            // Arrange
            _mockCheckInService
                .Setup(s => s.PerformCheckInAsync(It.IsAny<CheckInRequest>()))
                .ThrowsAsync(new ArgumentException("Cannot check in a cancelled booking."));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.Perform(
                    new CheckInRequest { ReferenceNumber = "BK20251201ABCDEF" }));
            Assert.That(ex!.Message, Does.Contain("cancelled"));
        }

        // ── STATUS ────────────────────────────────────────────────────────────

        [Test]
        public async Task Status_CheckedInBooking_Returns200WithStatusCompleted()
        {
            // Arrange
            var expected = new CheckInStatusResponse
            {
                ReferenceNumber = "BK20251201ABCDEF",
                IsCheckedIn     = true,
                SeatNumber      = "14C",
                PassengerName   = "John Doe",
                FlightNumber    = "AI101",
                CheckInStatus   = "CheckedIn"
            };
            _mockCheckInService
                .Setup(s => s.GetCheckInStatusAsync("BK20251201ABCDEF"))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Status("BK20251201ABCDEF");

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok     = (OkObjectResult)result;
            var status = (CheckInStatusResponse)ok.Value!;
            Assert.That(status.IsCheckedIn,  Is.True);
            Assert.That(status.SeatNumber,   Is.EqualTo("14C"));
        }

        [Test]
        public async Task Status_NotCheckedIn_Returns200WithIsCheckedInFalse()
        {
            // Arrange
            var expected = new CheckInStatusResponse
            {
                ReferenceNumber = "BK20251201XYZXYZ",
                IsCheckedIn     = false,
                SeatNumber      = null,
                CheckInStatus   = "Confirmed"
            };
            _mockCheckInService
                .Setup(s => s.GetCheckInStatusAsync("BK20251201XYZXYZ"))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Status("BK20251201XYZXYZ");

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok     = (OkObjectResult)result;
            var status = (CheckInStatusResponse)ok.Value!;
            Assert.That(status.IsCheckedIn, Is.False);
            Assert.That(status.SeatNumber,  Is.Null);
        }
    }
}
