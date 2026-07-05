using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Data;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class FlightRepository : IFlightRepository
    {
        private readonly FlightDbContext _context;

        public FlightRepository(FlightDbContext context) => _context = context;

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime date)
            => await _context.Flights
                .Where(f => f.Origin.ToLower() == origin.ToLower()
                         && f.Destination.ToLower() == destination.ToLower()
                         && f.FlightDate.Date == date.Date
                         && f.AvailableSeats > 0)
                .ToListAsync();

        public async Task<Flight?> GetByIdAsync(int flightId)
            => await _context.Flights.FindAsync(flightId);

        public async Task<Flight?> GetByFlightNumberAsync(string flightNumber)
            => await _context.Flights
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

        public async Task<Flight> AddAsync(Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task<Flight> UpdateAsync(Flight flight)
        {
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task DeleteAsync(int flightId)
        {
            var flight = await _context.Flights.FindAsync(flightId)
                ?? throw new KeyNotFoundException($"Flight with ID {flightId} not found.");
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
        }

        public async Task DecrementSeatsAsync(int flightId)
        {
            var flight = await _context.Flights.FindAsync(flightId)
                ?? throw new KeyNotFoundException($"Flight with ID {flightId} not found.");

            if (flight.AvailableSeats <= 0)
                throw new InvalidOperationException("No available seats on this flight.");

            flight.AvailableSeats--;
            await _context.SaveChangesAsync();
        }
    }
}
