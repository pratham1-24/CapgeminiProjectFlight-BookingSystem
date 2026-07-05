using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;

namespace FlightBookingSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(BookingCreateRequest request, int customerId);
        Task<BookingResponse> GetBookingByReferenceAsync(string referenceNumber);
    }
}
