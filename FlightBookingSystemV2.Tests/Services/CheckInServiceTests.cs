using Moq;
using NUnit.Framework;
using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Implementations;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Tests.Services
{
    /// <summary>
    /// Unit tests for CheckInService.
    /// Tests: PerformCheckIn (success, already checked in, not found, cancelled)
    ///        GetCheckInStatus (checked-in, not checked-in)
    /// </summary>
    [TestFixture]
    public class CheckInServiceTests
    {
        private Mock<IBookingRepository>  _mockBookingRepo  = null!;
        private Mock<ICheckInRepository>  _mockCheckInRepo  = null!;
        private Mock<IUserRepository>     _mockUserRepo     = null!;
        private Mock<IEmailService>       _mockEmail        = null!;
        private CheckInService            _checkInService   = null!;

        [SetUp]
        public void SetUp()
        {
            _mockBookingRepo = new Mock<IBookingRepository>();
            _mockCheckInRepo = new Mock<ICheckInRepository>();
            _mockUserRepo    = new Mock<IUserRepository>();
            _mockEmail       = new Mock<IEmailService>();
            _checkInService  = new CheckInService(
                _mockBookingRepo.Object,
                _mockCheckInRepo.Object,
                _mockUserRepo.Object,
                _mockEmail.Object);
        }

        private Booking MakeValidBooking(bool isCheckedIn = false, string status = "Confirmed") => new()
        {
            BookingId       = 1,
            ReferenceNumber = "BK20251201ABCDEF",
            CustomerId      = 1,
            FirstName       = "John",
            LastName        = "Doe",
            IsCheckedIn     = isCheckedIn,
            BookingStatus   = status,
            Flight = new Flight
            {
                FlightId     = 1,
                FlightNumber = "AI101",
                Origin       = "Mumbai",
                Destination  = "Delhi",
                FlightDate   = DateTime.UtcNow.AddDays(2)
            }
        };

        [Test]
        public async Task PerformCheckIn_ValidBooking_ReturnsSeatAndCompletedStatus()
        {
            // Arrange
            var booking  = MakeValidBooking();
            var customer = new Customer { Id = 1, Email = "john@gmail.com" };

            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("BK20251201ABCDEF"))
                .ReturnsAsync(booking);
            _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockCheckInRepo
                .Setup(r => r.CreateAsync(It.IsAny<CheckIn>()))
                .ReturnsAsync((CheckIn ci) => { ci.CheckInId = 1; return ci; });

            // Act
            var result = await _checkInService.PerformCheckInAsync(
                new CheckInRequest { ReferenceNumber = "BK20251201ABCDEF" });

            // Assert
            Assert.That(result.Message,       Is.EqualTo("Check-in successful!"));
            Assert.That(result.CheckInStatus, Is.EqualTo("Completed"));
            Assert.That(result.SeatNumber,    Is.Not.Null.And.Not.Empty);
            Assert.That(result.CheckInReference, Does.StartWith("CI"));
            Assert.That(result.PassengerName, Is.EqualTo("John Doe"));

            // Verify booking was updated
            _mockCheckInRepo.Verify(
                r => r.UpdateBookingCheckInAsync(1, It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Once);
        }

        [Test]
        public void PerformCheckIn_AlreadyCheckedIn_ThrowsArgumentException()
        {
            // Arrange — booking already has IsCheckedIn = true
            var booking = MakeValidBooking(isCheckedIn: true);
            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("BK20251201ABCDEF"))
                .ReturnsAsync(booking);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _checkInService.PerformCheckInAsync(
                    new CheckInRequest { ReferenceNumber = "BK20251201ABCDEF" }));
            Assert.That(ex!.Message, Is.EqualTo("Passenger is already checked in."));
        }

        [Test]
        public void PerformCheckIn_BookingNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("INVALID"))
                .ReturnsAsync((Booking?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _checkInService.PerformCheckInAsync(
                    new CheckInRequest { ReferenceNumber = "INVALID" }));
        }

        [Test]
        public void PerformCheckIn_CancelledBooking_ThrowsArgumentException()
        {
            // Arrange — booking was cancelled
            var booking = MakeValidBooking(isCheckedIn: false, status: "Cancelled");
            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("BK20251201ABCDEF"))
                .ReturnsAsync(booking);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _checkInService.PerformCheckInAsync(
                    new CheckInRequest { ReferenceNumber = "BK20251201ABCDEF" }));
            Assert.That(ex!.Message, Does.Contain("cancelled"));
        }

        [Test]
        public async Task GetCheckInStatus_CheckedInBooking_ReturnsIsCheckedInTrue()
        {
            // Arrange
            var booking = MakeValidBooking(isCheckedIn: true, status: "CheckedIn");
            booking.SeatNumber = "14C";
            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("BK20251201ABCDEF"))
                .ReturnsAsync(booking);

            // Act
            var result = await _checkInService.GetCheckInStatusAsync("BK20251201ABCDEF");

            // Assert
            Assert.That(result.IsCheckedIn,   Is.True);
            Assert.That(result.SeatNumber,    Is.EqualTo("14C"));
            Assert.That(result.CheckInStatus, Is.EqualTo("CheckedIn"));
        }

        [Test]
        public async Task GetCheckInStatus_NotCheckedIn_ReturnsIsCheckedInFalse()
        {
            // Arrange
            var booking = MakeValidBooking(isCheckedIn: false);
            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("BK20251201ABCDEF"))
                .ReturnsAsync(booking);

            // Act
            var result = await _checkInService.GetCheckInStatusAsync("BK20251201ABCDEF");

            // Assert
            Assert.That(result.IsCheckedIn, Is.False);
            Assert.That(result.SeatNumber,  Is.Null);
        }
    }
}
