using FlightBookingSystem.Models.Entities;

namespace FlightBookingSystem.Repositories.Interfaces
{
    public interface IAdminFlightRepository
    {
        Task<Flight> AddFlightAsync(Flight flight);
        Task<Flight> UpdateFlightAsync(Flight flight);
        Task DeleteFlightAsync(int flightId);
        Task<Admin?> ValidateAdminAsync(string username, string password);
    }
}
