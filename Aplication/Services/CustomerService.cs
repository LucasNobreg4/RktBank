using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces;
using Application.Constants;
using Application.Util;

namespace Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCacheService _cacheService;

        public CustomerService(
            ICustomerRepository customerRepository,
            IUserRepository userRepository,
            IMemoryCacheService cacheService)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        private async Task EnsureCacheLoadedAsync()
        {
            if (_cacheService.TryGetValue<bool>(CostumersKeys.Initialized, out _))
            {
                return;
            }

            var allCustomers = await _customerRepository.GetAllAsync();
            var allDtos = allCustomers.Select(CustomerUtil.MapToDto).ToList();

            _cacheService.SetPermanent(CostumersKeys.All, allDtos);

            foreach (var customer in allCustomers)
            {
                var dto = CustomerUtil.MapToDto(customer);
                _cacheService.SetPermanent(CostumersKeys.ById(customer.Id), dto);
                _cacheService.SetPermanent(CostumersKeys.ByDocument(customer.Document), dto);
            }

            _cacheService.SetPermanent(CostumersKeys.Initialized, true);
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

            await EnsureCacheLoadedAsync();

            if (_cacheService.TryGetValue<CustomerDto>(CostumersKeys.ByDocument(dto.Document), out _))
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
            var customerDto = CustomerUtil.MapToDto(createdCustomer);

            _cacheService.SetPermanent(CostumersKeys.ById(createdCustomer.Id), customerDto);
            _cacheService.SetPermanent(CostumersKeys.ByDocument(createdCustomer.Document), customerDto);
            _cacheService.Remove(CostumersKeys.All);

            return new CustomerResponseDto
            {
                Success = true,
                Message = "Cliente criado com sucesso.",
                Customer = customerDto
            };
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(string id)
        {
            await EnsureCacheLoadedAsync();

            if (_cacheService.TryGetValue<CustomerDto>(CostumersKeys.ById(id), out var customer))
            {
                return customer;
            }

            return null;
        }

        public async Task<CustomerDto?> GetCustomerByDocumentAsync(string document)
        {
            await EnsureCacheLoadedAsync();

            if (_cacheService.TryGetValue<CustomerDto>(CostumersKeys.ByDocument(document), out var customer))
            {
                return customer;
            }

            return null;
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            await EnsureCacheLoadedAsync();

            if (_cacheService.TryGetValue<List<CustomerDto>>(CostumersKeys.All, out var customers))
            {
                return customers;
            }

            var allCustomers = await _customerRepository.GetAllAsync();
            var dtos = allCustomers.Select(CustomerUtil.MapToDto).ToList();
            _cacheService.SetPermanent(CostumersKeys.All, dtos);

            return dtos;
        }

        public async Task<List<CustomerDto>> GetCustomersByUserAsync(string userId)
        {
            await EnsureCacheLoadedAsync();

            var allCustomers = await GetAllCustomersAsync();
            return allCustomers.Where(c => c.CreatedByUserId == userId).ToList();
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

            var customerDto = CustomerUtil.MapToDto(customer);
            _cacheService.SetPermanent(CostumersKeys.ById(customer.Id), customerDto);
            _cacheService.SetPermanent(CostumersKeys.ByDocument(customer.Document), customerDto);
            _cacheService.Remove(CostumersKeys.All);

            return new CustomerResponseDto
            {
                Success = true,
                Message = "Cliente atualizado com sucesso.",
                Customer = customerDto
            };
        }

        public async Task<bool> DeactivateCustomerAsync(string id)
        {
            var result = await _customerRepository.DeleteAsync(id);

            if (result)
            {
                if (_cacheService.TryGetValue<CustomerDto>(CostumersKeys.ById(id), out var customer))
                {
                    _cacheService.Remove(CostumersKeys.ById(id));
                    _cacheService.Remove(CostumersKeys.ByDocument(customer.Document));
                }

                _cacheService.Remove(CostumersKeys.All);
            }

            return result;
        }
    }
}
