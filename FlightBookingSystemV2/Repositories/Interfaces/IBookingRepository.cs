using FlightBookingSystem.Models.Entities;

namespace FlightBookingSystem.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking?> GetByReferenceAsync(string referenceNumber);
        Task<Booking?> GetByIdWithDetailsAsync(int bookingId);
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
        Task<Booking> UpdateAsync(Booking booking);
        Task DeleteAsync(int bookingId);
    }
}
