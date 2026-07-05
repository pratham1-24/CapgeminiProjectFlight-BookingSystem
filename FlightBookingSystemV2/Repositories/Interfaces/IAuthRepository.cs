using FlightBookingSystem.Models.Entities;

namespace FlightBookingSystem.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<Customer?> ValidateCustomerAsync(string username, string password);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<Customer> RegisterCustomerAsync(string username, string email, string hashedPassword);
    }
}
