using FlightBookingSystem.Models.Entities;

namespace FlightBookingSystem.Repositories.Interfaces
{
    public interface IFareRepository
    {
        Task<Flight?> GetFareByFlightNumberAsync(string flightNumber);
        Task<Flight?> GetFareByFlightIdAsync(int flightId);
    }
}
