using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Services.Implementations
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _flightRepo;

        public FlightService(IFlightRepository flightRepo) => _flightRepo = flightRepo;

        public async Task<IEnumerable<FlightResponse>> SearchFlightsAsync(FlightSearchRequest request)
        {
            if (request.Origin.Equals(request.Destination, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Origin and destination cannot be the same.");

            if (request.FlightDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Flight date must be today or a future date.");

            var flights = await _flightRepo.SearchFlightsAsync(request.Origin, request.Destination, request.FlightDate);

            return flights.Select(f => new FlightResponse
            {
                FlightId       = f.FlightId,
                FlightNumber   = f.FlightNumber,
                Origin         = f.Origin,
                Destination    = f.Destination,
                FlightDate     = f.FlightDate,
                Fare           = f.Fare,
                AvailableSeats = f.AvailableSeats
            });
        }
    }
}
