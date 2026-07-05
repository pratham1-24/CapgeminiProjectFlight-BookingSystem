using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Data;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class FareRepository : IFareRepository
    {
        private readonly FlightDbContext _context;

        public FareRepository(FlightDbContext context) => _context = context;

        public async Task<Flight?> GetFareByFlightNumberAsync(string flightNumber)
            => await _context.Flights
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

        public async Task<Flight?> GetFareByFlightIdAsync(int flightId)
            => await _context.Flights.FindAsync(flightId);
    }
}
