using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Data;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly FlightDbContext _context;

        public AuthRepository(FlightDbContext context) => _context = context;

        public async Task<Customer?> ValidateCustomerAsync(string username, string password)
            => await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

        public async Task<bool> UsernameExistsAsync(string username)
            => await _context.Customers.AnyAsync(c => c.Username == username);

        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Customers.AnyAsync(c => c.Email == email);

        public async Task<Customer> RegisterCustomerAsync(string username, string email, string hashedPassword)
        {
            var customer = new Customer
            {
                Username = username,
                Email = email,
                Password = hashedPassword
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }
    }
}
