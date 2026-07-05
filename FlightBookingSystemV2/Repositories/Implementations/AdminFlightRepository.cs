using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Data;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;
using BCrypt.Net;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class AdminFlightRepository : IAdminFlightRepository
    {
        private readonly FlightDbContext _context;

        public AdminFlightRepository(FlightDbContext context) => _context = context;

        public async Task<Admin?> ValidateAdminAsync(string username, string password)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username);

            if (admin == null) return null;

            // Verify BCrypt hashed password
            return BCrypt.Net.BCrypt.Verify(password, admin.Password) ? admin : null;
        }

        public async Task<Flight> AddFlightAsync(Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task<Flight> UpdateFlightAsync(Flight flight)
        {
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task DeleteFlightAsync(int flightId)
        {
            var flight = await _context.Flights.FindAsync(flightId)
                ?? throw new KeyNotFoundException($"Flight with ID {flightId} not found.");

            var hasBookings = await _context.Bookings.AnyAsync(b => b.FlightId == flightId);
            if (hasBookings)
                throw new InvalidOperationException("Cannot delete a flight that has existing bookings.");

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
        }
    }
}
