using FlightBookingSystem.Models.Entities;

namespace FlightBookingSystem.Repositories.Interfaces
{
    public interface ICheckInRepository
    {
        Task<CheckIn> CreateAsync(CheckIn checkIn);
        Task<CheckIn?> GetByBookingIdAsync(int bookingId);
        Task<CheckIn?> GetByReferenceAsync(string checkInReference);
        Task UpdateBookingCheckInAsync(int bookingId, string seatNumber, DateTime checkInDate);
    }
}
