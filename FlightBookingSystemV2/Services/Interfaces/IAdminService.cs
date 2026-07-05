using FlightBookingSystem.Models.DTOs.Requests;
using FlightBookingSystem.Models.DTOs.Responses;

namespace FlightBookingSystem.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request);
        Task<FlightResponse> CreateFlightAsync(FlightCreateRequest request);
        Task<FlightResponse> UpdateFlightAsync(FlightUpdateRequest request);
        Task DeleteFlightAsync(int flightId);
        Task<SuccessResponse> UpdateUserAsync(UserUpdateRequest request);
    }
}
