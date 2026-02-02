using Domain.Entities;

namespace Infrastructure.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(string id);
        Task<Customer?> GetByDocumentAsync(string document);
        Task<Customer?> GetByEmailAsync(string email);
        Task<List<Customer>> GetAllAsync();
        Task<List<Customer>> GetByCreatedByUserIdAsync(string userId);
        Task<Customer> CreateAsync(Customer customer);
        Task<bool> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(string id);
    }
}
