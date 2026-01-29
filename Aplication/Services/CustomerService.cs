using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

        public CustomerService(
            ICustomerRepository customerRepository,
            IUserRepository userRepository)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
        }

        public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto, string createdByUserId)
        {
            var user = await _userRepository.GetByIdAsync(createdByUserId);
            if (user == null)
            {
                return new CustomerResponseDto
                {
                    Success = false,
                    Message = "Usuário não encontrado."
                };
            }

            var existingCustomer = await _customerRepository.GetByDocumentAsync(dto.Document);
            if (existingCustomer != null)
            {
                return new CustomerResponseDto
                {
                    Success = false,
                    Message = "Cliente com este documento já existe."
                };
            }

            if (!Enum.TryParse<CustomerType>(dto.Type, out var customerType))
            {
                return new CustomerResponseDto
                {
                    Success = false,
                    Message = "Tipo de cliente inválido. Use 'Individual' ou 'Business'."
                };
            }

            var customer = new Customer
            {
                Document = dto.Document,
                Type = customerType,
                FullName = dto.FullName,
                BirthDate = dto.BirthDate,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                CreatedByUserId = createdByUserId
            };

            var createdCustomer = await _customerRepository.CreateAsync(customer);

            return new CustomerResponseDto
            {
                Success = true,
                Message = "Cliente criado com sucesso.",
                Customer = MapToDto(createdCustomer)
            };
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return customer != null ? MapToDto(customer) : null;
        }

        public async Task<CustomerDto?> GetCustomerByDocumentAsync(string document)
        {
            var customer = await _customerRepository.GetByDocumentAsync(document);
            return customer != null ? MapToDto(customer) : null;
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(MapToDto).ToList();
        }

        public async Task<List<CustomerDto>> GetCustomersByUserAsync(string userId)
        {
            var customers = await _customerRepository.GetByCreatedByUserIdAsync(userId);
            return customers.Select(MapToDto).ToList();
        }

        public async Task<CustomerResponseDto> UpdateCustomerAsync(string id, UpdateCustomerDto dto)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return new CustomerResponseDto
                {
                    Success = false,
                    Message = "Cliente não encontrado."
                };
            }

            customer.FullName = dto.FullName;
            customer.Email = dto.Email;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.Address = dto.Address;
            customer.City = dto.City;
            customer.State = dto.State;
            customer.ZipCode = dto.ZipCode;

            var success = await _customerRepository.UpdateAsync(customer);

            if (!success)
            {
                return new CustomerResponseDto
                {
                    Success = false,
                    Message = "Falha ao atualizar cliente."
                };
            }

            return new CustomerResponseDto
            {
                Success = true,
                Message = "Cliente atualizado com sucesso.",
                Customer = MapToDto(customer)
            };
        }

        public async Task<bool> DeactivateCustomerAsync(string id)
        {
            return await _customerRepository.DeleteAsync(id);
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Document = customer.Document,
                Type = customer.Type.ToString(),
                FullName = customer.FullName,
                BirthDate = customer.BirthDate,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode,
                CreatedAt = customer.CreatedAt,
                IsActive = customer.IsActive
            };
        }
    }
}
