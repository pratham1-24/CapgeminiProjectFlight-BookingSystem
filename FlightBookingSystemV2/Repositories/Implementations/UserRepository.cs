using Microsoft.EntityFrameworkCore;
using FlightBookingSystem.Data;
using FlightBookingSystem.Models.Entities;
using FlightBookingSystem.Repositories.Interfaces;

namespace FlightBookingSystem.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly FlightDbContext _context;

        public UserRepository(FlightDbContext context) => _context = context;

        public async Task<Customer?> GetByUsernameAsync(string username)
            => await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);

        public async Task<Customer?> GetByIdAsync(int id)
            => await _context.Customers.FindAsync(id);

        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }
    }
}
