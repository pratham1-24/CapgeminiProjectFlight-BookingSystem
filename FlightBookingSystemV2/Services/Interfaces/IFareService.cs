using FlightBookingSystem.Models.DTOs.Responses;

namespace FlightBookingSystem.Services.Interfaces
{
    public interface IFareService
    {
        Task<FareResponse> GetFareByFlightNumberAsync(string flightNumber);
        Task<FareResponse> GetFareByFlightIdAsync(int flightId);
    }
}
