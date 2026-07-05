using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Data;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class CheckInRepository : ICheckInRepository
    {
        private readonly FlightDbContext _context;

        public CheckInRepository(FlightDbContext context) => _context = context;

        public async Task<CheckIn> CreateAsync(CheckIn checkIn)
        {
            _context.CheckIns.Add(checkIn);
            await _context.SaveChangesAsync();
            return checkIn;
        }

        public async Task<CheckIn?> GetByBookingIdAsync(int bookingId)
            => await _context.CheckIns.FirstOrDefaultAsync(c => c.BookingId == bookingId);

        public async Task<CheckIn?> GetByReferenceAsync(string checkInReference)
            => await _context.CheckIns
                .Include(c => c.Booking)
                    .ThenInclude(b => b!.Flight)
                .FirstOrDefaultAsync(c => c.CheckInReference == checkInReference);

        public async Task UpdateBookingCheckInAsync(int bookingId, string seatNumber, DateTime checkInDate)
        {
            var booking = await _context.Bookings.FindAsync(bookingId)
                ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

            booking.IsCheckedIn = true;
            booking.SeatNumber = seatNumber;
            booking.CheckInDate = checkInDate;
            booking.BookingStatus = "CheckedIn";

            await _context.SaveChangesAsync();
        }
    }
}
