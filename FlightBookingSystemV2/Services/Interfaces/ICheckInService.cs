using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;

namespace FlightBookingSystem.Services.Interfaces
{
    public interface ICheckInService
    {
        Task<BookingResponse> SearchBookingAsync(string referenceNumber);
        Task<CheckInResponse> PerformCheckInAsync(CheckInRequest request);
        Task<CheckInStatusResponse> GetCheckInStatusAsync(string referenceNumber);
    }
}
