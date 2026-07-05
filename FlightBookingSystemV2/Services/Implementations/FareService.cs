using FlightBookingSystem.Models.DTOs.Responses;
using FlightBookingSystem.Repositories.Interfaces;
using FlightBookingSystem.Services.Interfaces;

namespace FlightBookingSystem.Services.Implementations
{
    public class FareService : IFareService
    {
        private readonly IFareRepository _fareRepo;
        private const decimal GstRate = 0.18m; // 18% GST

        public FareService(IFareRepository fareRepo) => _fareRepo = fareRepo;

        public async Task<FareResponse> GetFareByFlightNumberAsync(string flightNumber)
        {
            var flight = await _fareRepo.GetFareByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"Flight '{flightNumber}' not found.");

            var gst = Math.Round(flight.Fare * GstRate, 2);
            return new FareResponse
            {
                FlightId     = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                Origin       = flight.Origin,
                Destination  = flight.Destination,
                FlightDate   = flight.FlightDate,
                BaseFare     = flight.Fare,
                GstAmount    = gst,
                FinalFare    = flight.Fare + gst
            };
        }

        public async Task<FareResponse> GetFareByFlightIdAsync(int flightId)
        {
            var flight = await _fareRepo.GetFareByFlightIdAsync(flightId)
                ?? throw new KeyNotFoundException($"Flight ID {flightId} not found.");

            var gst = Math.Round(flight.Fare * GstRate, 2);
            return new FareResponse
            {
                FlightId     = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                Origin       = flight.Origin,
                Destination  = flight.Destination,
                FlightDate   = flight.FlightDate,
                BaseFare     = flight.Fare,
                GstAmount    = gst,
                FinalFare    = flight.Fare + gst
            };
        }
    }
}
