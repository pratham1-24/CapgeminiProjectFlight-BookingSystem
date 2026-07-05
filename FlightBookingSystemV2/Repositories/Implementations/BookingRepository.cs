using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Data;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly FlightDbContext _context;

        public BookingRepository(FlightDbContext context) => _context = context;

        public async Task<Booking> CreateAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> GetByReferenceAsync(string referenceNumber)
            => await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber);

        public async Task<Booking?> GetByIdWithDetailsAsync(int bookingId)
            => await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Customer)
                .Include(b => b.CheckIn)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
            => await _context.Bookings
                .Include(b => b.Flight)
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task DeleteAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId)
                ?? throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }
}
