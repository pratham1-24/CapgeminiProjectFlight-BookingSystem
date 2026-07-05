using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;

namespace FlightBookingSystem.Services.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<FlightResponse>> SearchFlightsAsync(FlightSearchRequest request);
    }
}
