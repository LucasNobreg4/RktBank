using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(string id)
        {
            return await _context.Customers
                .Include(c => c.CreatedByUser)
                .Include(c => c.BankAccounts)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByDocumentAsync(string document)
        {
            return await _context.Customers
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Document == document);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.CreatedByUser)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Customer>> GetByCreatedByUserIdAsync(string userId)
        {
            return await _context.Customers
                .Where(c => c.CreatedByUserId == userId && c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            customer.UpdatedAt = DateTime.UtcNow;
            _context.Customers.Update(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var customer = await GetByIdAsync(id);
            if (customer == null) return false;

            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            return await UpdateAsync(customer);
        }
    }
}
