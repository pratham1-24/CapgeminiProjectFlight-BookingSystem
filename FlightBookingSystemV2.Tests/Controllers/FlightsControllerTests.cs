using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using FlightBookingSystem.Controllers;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Tests.Controllers
{
    /// <summary>
    /// Unit tests for FlightsController.
    /// Tests: Search (results found, empty list, same origin/destination, past date)
    /// </summary>
    [TestFixture]
    public class FlightsControllerTests
    {
        private Mock<IFlightService> _mockFlightService = null!;
        private FlightsController    _controller        = null!;

        [SetUp]
        public void SetUp()
        {
            _mockFlightService = new Mock<IFlightService>();
            _controller        = new FlightsController(_mockFlightService.Object);
        }

        [Test]
        public async Task Search_ValidRoute_Returns200WithFlights()
        {
            // Arrange
            var flights = new List<FlightResponse>
            {
                new() { FlightId = 1, FlightNumber = "AI101", Origin = "Mumbai",
                        Destination = "Delhi", Fare = 4500, AvailableSeats = 120 },
                new() { FlightId = 2, FlightNumber = "AI102", Origin = "Mumbai",
                        Destination = "Delhi", Fare = 5200, AvailableSeats = 80  }
            };
            _mockFlightService
                .Setup(s => s.SearchFlightsAsync(It.IsAny<Models.DTOs.Requests.FlightSearchRequest>()))
                .ReturnsAsync(flights);

            // Act
            var result = await _controller.Search("Mumbai", "Delhi", DateTime.UtcNow.AddDays(5));

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok = (OkObjectResult)result;
            var list = (IEnumerable<FlightResponse>)ok.Value!;
            Assert.That(list.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Search_NoFlightsFound_Returns200WithEmptyList()
        {
            // Arrange — empty list, not 404
            _mockFlightService
                .Setup(s => s.SearchFlightsAsync(It.IsAny<Models.DTOs.Requests.FlightSearchRequest>()))
                .ReturnsAsync(new List<FlightResponse>());

            // Act
            var result = await _controller.Search("Jaipur", "Kochi", DateTime.UtcNow.AddDays(5));

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok = (OkObjectResult)result;
            var list = (IEnumerable<FlightResponse>)ok.Value!;
            Assert.That(list, Is.Empty);
        }

        [Test]
        public async Task Search_SameOriginDestination_ThrowsArgumentException()
        {
            // Arrange
            _mockFlightService
                .Setup(s => s.SearchFlightsAsync(It.IsAny<Models.DTOs.Requests.FlightSearchRequest>()))
                .ThrowsAsync(new ArgumentException("Origin and destination cannot be the same."));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.Search("Delhi", "Delhi", DateTime.UtcNow.AddDays(1)));
            Assert.That(ex!.Message, Does.Contain("same"));
        }

        [Test]
        public async Task Search_PastDate_ThrowsArgumentException()
        {
            // Arrange
            _mockFlightService
                .Setup(s => s.SearchFlightsAsync(It.IsAny<Models.DTOs.Requests.FlightSearchRequest>()))
                .ThrowsAsync(new ArgumentException("Flight date must be today or a future date."));

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.Search("Mumbai", "Delhi", DateTime.UtcNow.AddDays(-5)));
        }
    }
}
