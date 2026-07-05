using FlightBookingSystem.Models.Entities;

namespace FlightBookingSystem.Repositories.Interfaces
{
    public interface IFlightRepository
    {
        Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime date);
        Task<Flight?> GetByIdAsync(int flightId);
        Task<Flight?> GetByFlightNumberAsync(string flightNumber);
        Task<Flight> AddAsync(Flight flight);
        Task<Flight> UpdateAsync(Flight flight);
        Task DeleteAsync(int flightId);
        Task DecrementSeatsAsync(int flightId);
    }
}
