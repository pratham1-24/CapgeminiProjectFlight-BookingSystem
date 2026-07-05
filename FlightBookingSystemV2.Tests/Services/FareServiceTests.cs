using Moq;
using NUnit.Framework;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Implementations;

namespace FlightBookingSystem.Tests.Services
{
    /// <summary>
    /// Unit tests for FareService.
    /// Tests: GetFareByFlightNumber (found, not found)
    ///        GetFareByFlightId     (found, not found)
    /// Verifies 18% GST is correctly calculated.
    /// </summary>
    [TestFixture]
    public class FareServiceTests
    {
        private Mock<IFareRepository> _mockFareRepo = null!;
        private FareService           _fareService  = null!;

        [SetUp]
        public void SetUp()
        {
            _mockFareRepo = new Mock<IFareRepository>();
            _fareService  = new FareService(_mockFareRepo.Object);
        }

        // ── BY FLIGHT NUMBER ──────────────────────────────────────────────────

        [Test]
        public async Task GetFareByFlightNumber_ValidFlight_ReturnsCorrectGstCalculation()
        {
            // Arrange
            var flight = new Flight
            {
                FlightId     = 1,
                FlightNumber = "AI101",
                Origin       = "Mumbai",
                Destination  = "Delhi",
                FlightDate   = DateTime.UtcNow.AddDays(5),
                Fare         = 4500.00m
            };
            _mockFareRepo.Setup(r => r.GetFareByFlightNumberAsync("AI101")).ReturnsAsync(flight);

            // Act
            var result = await _fareService.GetFareByFlightNumberAsync("AI101");

            // Assert
            Assert.That(result.FlightNumber, Is.EqualTo("AI101"));
            Assert.That(result.BaseFare,     Is.EqualTo(4500.00m));
            Assert.That(result.GstAmount,    Is.EqualTo(810.00m));    // 4500 * 18% = 810
            Assert.That(result.FinalFare,    Is.EqualTo(5310.00m));   // 4500 + 810 = 5310
        }

        [Test]
        [TestCase(1000.00, 180.00, 1180.00)]   // 18% of 1000 = 180
        [TestCase(3200.00, 576.00, 3776.00)]   // 18% of 3200 = 576
        [TestCase(8500.00, 1530.00, 10030.00)] // 18% of 8500 = 1530
        public async Task GetFareByFlightNumber_DifferentFares_GstAlwaysCorrect(
            decimal baseFare, decimal expectedGst, decimal expectedFinal)
        {
            // Arrange
            var flight = new Flight
            {
                FlightId     = 1,
                FlightNumber = "FL100",
                Origin       = "A",
                Destination  = "B",
                FlightDate   = DateTime.UtcNow.AddDays(3),
                Fare         = baseFare
            };
            _mockFareRepo.Setup(r => r.GetFareByFlightNumberAsync("FL100")).ReturnsAsync(flight);

            // Act
            var result = await _fareService.GetFareByFlightNumberAsync("FL100");

            // Assert
            Assert.That(result.BaseFare,  Is.EqualTo(baseFare));
            Assert.That(result.GstAmount, Is.EqualTo(expectedGst));
            Assert.That(result.FinalFare, Is.EqualTo(expectedFinal));
        }

        [Test]
        public void GetFareByFlightNumber_InvalidFlight_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockFareRepo
                .Setup(r => r.GetFareByFlightNumberAsync("INVALID"))
                .ReturnsAsync((Flight?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _fareService.GetFareByFlightNumberAsync("INVALID"));
            Assert.That(ex!.Message, Does.Contain("INVALID"));
        }

        // ── BY FLIGHT ID ──────────────────────────────────────────────────────

        [Test]
        public async Task GetFareByFlightId_ValidId_ReturnsGstBreakdown()
        {
            // Arrange
            var flight = new Flight
            {
                FlightId     = 5,
                FlightNumber = "AI505",
                Origin       = "Hyderabad",
                Destination  = "Bangalore",
                Fare         = 2500.00m
            };
            _mockFareRepo.Setup(r => r.GetFareByFlightIdAsync(5)).ReturnsAsync(flight);

            // Act
            var result = await _fareService.GetFareByFlightIdAsync(5);

            // Assert
            Assert.That(result.FlightId,  Is.EqualTo(5));
            Assert.That(result.BaseFare,  Is.EqualTo(2500.00m));
            Assert.That(result.GstAmount, Is.EqualTo(450.00m));   // 2500 * 18% = 450
            Assert.That(result.FinalFare, Is.EqualTo(2950.00m));  // 2500 + 450 = 2950
        }

        [Test]
        public void GetFareByFlightId_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockFareRepo
                .Setup(r => r.GetFareByFlightIdAsync(999))
                .ReturnsAsync((Flight?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _fareService.GetFareByFlightIdAsync(999));
        }
    }
}
