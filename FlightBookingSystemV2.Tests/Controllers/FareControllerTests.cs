using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using FlightBookingSystem.Controllers;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Tests.Controllers
{
    /// <summary>
    /// Unit tests for FareController.
    /// Tests: GetFareByFlightNumber (found, not found, empty input)
    ///        GetFareByFlightId     (found, not found)
    /// Verifies GST calculation: FinalFare = BaseFare + 18%
    /// </summary>
    [TestFixture]
    public class FareControllerTests
    {
        private Mock<IFareService> _mockFareService = null!;
        private FareController     _controller      = null!;

        [SetUp]
        public void SetUp()
        {
            _mockFareService = new Mock<IFareService>();
            _controller      = new FareController(_mockFareService.Object);
        }

        // ── BY FLIGHT NUMBER ──────────────────────────────────────────────────

        [Test]
        public async Task GetFareByFlightNumber_ValidNumber_Returns200WithGstBreakdown()
        {
            // Arrange
            var expected = new FareResponse
            {
                FlightId     = 1,
                FlightNumber = "AI101",
                Origin       = "Mumbai",
                Destination  = "Delhi",
                BaseFare     = 4500.00m,
                GstAmount    = 810.00m,   // 18% of 4500
                FinalFare    = 5310.00m   // 4500 + 810
            };
            _mockFareService
                .Setup(s => s.GetFareByFlightNumberAsync("AI101"))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetFareByFlightNumber("AI101");

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok       = (OkObjectResult)result;
            var fare     = (FareResponse)ok.Value!;
            Assert.That(fare.FlightNumber, Is.EqualTo("AI101"));
            Assert.That(fare.BaseFare,     Is.EqualTo(4500.00m));
            Assert.That(fare.GstAmount,    Is.EqualTo(810.00m));
            Assert.That(fare.FinalFare,    Is.EqualTo(5310.00m));

            // Verify: FinalFare = BaseFare + GstAmount
            Assert.That(fare.FinalFare, Is.EqualTo(fare.BaseFare + fare.GstAmount));
        }

        [Test]
        public async Task GetFareByFlightNumber_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockFareService
                .Setup(s => s.GetFareByFlightNumberAsync("INVALID"))
                .ThrowsAsync(new KeyNotFoundException("Flight 'INVALID' not found."));

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _controller.GetFareByFlightNumber("INVALID"));
        }

        [Test]
        public async Task GetFareByFlightNumber_EmptyString_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetFareByFlightNumber("   ");

            // Assert — empty flight number should return 400
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ── BY FLIGHT ID ──────────────────────────────────────────────────────

        [Test]
        public async Task GetFareByFlightId_ValidId_Returns200()
        {
            // Arrange
            var expected = new FareResponse
            {
                FlightId  = 1,
                BaseFare  = 3200.00m,
                GstAmount = 576.00m,
                FinalFare = 3776.00m
            };
            _mockFareService
                .Setup(s => s.GetFareByFlightIdAsync(1))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetFareByFlightId(1);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok   = (OkObjectResult)result;
            var fare = (FareResponse)ok.Value!;
            Assert.That(fare.FinalFare, Is.EqualTo(3776.00m));
        }

        [Test]
        public async Task GetFareByFlightId_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockFareService
                .Setup(s => s.GetFareByFlightIdAsync(999))
                .ThrowsAsync(new KeyNotFoundException("Flight ID 999 not found."));

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _controller.GetFareByFlightId(999));
        }
    }
}
