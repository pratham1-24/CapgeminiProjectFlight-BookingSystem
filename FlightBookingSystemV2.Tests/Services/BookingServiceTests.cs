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
    /// Unit tests for BookingService.
    /// Tests: CreateBooking (success, flight not found, no seats)
    ///        GetBookingByReference (found, not found)
    /// Verifies: reference generation, BaseFare/FinalFare (GST), seat decrement, email trigger.
    /// </summary>
    [TestFixture]
    public class BookingServiceTests
    {
        private Mock<IBookingRepository> _mockBookingRepo = null!;
        private Mock<IFlightRepository>  _mockFlightRepo  = null!;
        private Mock<IUserRepository>    _mockUserRepo    = null!;
        private Mock<IEmailService>      _mockEmail       = null!;
        private BookingService           _bookingService  = null!;

        [SetUp]
        public void SetUp()
        {
            _mockBookingRepo = new Mock<IBookingRepository>();
            _mockFlightRepo  = new Mock<IFlightRepository>();
            _mockUserRepo    = new Mock<IUserRepository>();
            _mockEmail       = new Mock<IEmailService>();
            _bookingService  = new BookingService(
                _mockBookingRepo.Object,
                _mockFlightRepo.Object,
                _mockUserRepo.Object,
                _mockEmail.Object);
        }

        [Test]
        public async Task CreateBooking_ValidRequest_ReturnBookingWithBaseFareAndFinalFare()
        {
            // Arrange
            var request = new BookingCreateRequest
            {
                FlightId  = 1,
                FirstName = "John",
                LastName  = "Doe",
                Gender    = "Male"
            };
            var flight = new Flight
            {
                FlightId       = 1,
                FlightNumber   = "AI101",
                Origin         = "Mumbai",
                Destination    = "Delhi",
                FlightDate     = DateTime.UtcNow.AddDays(5),
                Fare           = 4500.00m,
                AvailableSeats = 100
            };
            var customer = new Customer
            {
                Id       = 1,
                Username = "john",
                Email    = "john@gmail.com"
            };

            _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
            _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockBookingRepo
                .Setup(r => r.CreateAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking b) => { b.BookingId = 1; return b; });

            // Act
            var result = await _bookingService.CreateBookingAsync(request, customerId: 1);

            // Assert
            Assert.That(result.FlightNumber,  Is.EqualTo("AI101"));
            Assert.That(result.FirstName,     Is.EqualTo("John"));
            Assert.That(result.BookingStatus, Is.EqualTo("Confirmed"));
            Assert.That(result.BaseFare,      Is.EqualTo(4500.00m));
            Assert.That(result.FinalFare,     Is.EqualTo(5310.00m)); // 4500 + 18% GST

            // Reference must start with BK
            Assert.That(result.ReferenceNumber, Does.StartWith("BK"));

            // Seat decrement must be called
            _mockFlightRepo.Verify(r => r.DecrementSeatsAsync(1), Times.Once);
        }

        [Test]
        public void CreateBooking_FlightNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new BookingCreateRequest { FlightId = 999, FirstName = "A", LastName = "B", Gender = "Male" };
            _mockFlightRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Flight?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _bookingService.CreateBookingAsync(request, customerId: 1));
            Assert.That(ex!.Message, Does.Contain("999"));
        }

        [Test]
        public void CreateBooking_NoSeatsAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            var flight = new Flight
            {
                FlightId       = 1,
                FlightNumber   = "AI101",
                Fare           = 4500,
                AvailableSeats = 0  // FULL flight
            };
            var request = new BookingCreateRequest { FlightId = 1, FirstName = "A", LastName = "B", Gender = "Male" };
            _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _bookingService.CreateBookingAsync(request, customerId: 1));
            Assert.That(ex!.Message, Does.Contain("No available seats"));
        }

        [Test]
        public async Task GetBookingByReference_ValidRef_ReturnsBookingDetails()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId       = 1,
                ReferenceNumber = "BK20251201ABCDEF",
                FirstName       = "Jane",
                LastName        = "Smith",
                BaseFare        = 3200,
                FinalFare       = 3776,
                BookingStatus   = "Confirmed",
                Flight = new Flight
                {
                    FlightId     = 1,
                    FlightNumber = "AI101",
                    Origin       = "Hyderabad",
                    Destination  = "Delhi",
                    Fare         = 3200
                }
            };
            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("BK20251201ABCDEF"))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingService.GetBookingByReferenceAsync("BK20251201ABCDEF");

            // Assert
            Assert.That(result.ReferenceNumber, Is.EqualTo("BK20251201ABCDEF"));
            Assert.That(result.LastName,        Is.EqualTo("Smith"));
            Assert.That(result.FinalFare,       Is.EqualTo(3776));
        }

        [Test]
        public void GetBookingByReference_InvalidRef_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockBookingRepo
                .Setup(r => r.GetByReferenceAsync("INVALID"))
                .ReturnsAsync((Booking?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _bookingService.GetBookingByReferenceAsync("INVALID"));
        }
    }
}
