using FlightBookingSystem.Data;
using FlightBookingSystem.Repositories.Interfaces;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class DeleteRepository : IDeleteRepository
    {
        private readonly FlightDbContext _context;

        public DeleteRepository(FlightDbContext context) => _context = context;

        public async Task DeleteFlightAsync(int flightId)
        {
            var flight = await _context.Flights.FindAsync(flightId)
                ?? throw new KeyNotFoundException($"Flight ID {flightId} not found.");
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId)
                ?? throw new KeyNotFoundException($"Booking ID {bookingId} not found.");
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }
}
