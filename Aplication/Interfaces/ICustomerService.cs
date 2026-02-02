using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto, string createdByUserId);
        Task<CustomerDto?> GetCustomerByIdAsync(string id);
        Task<CustomerDto?> GetCustomerByDocumentAsync(string document);
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<List<CustomerDto>> GetCustomersByUserAsync(string userId);
        Task<CustomerResponseDto> UpdateCustomerAsync(string id, UpdateCustomerDto dto);
        Task<bool> DeactivateCustomerAsync(string id);
    }
}
