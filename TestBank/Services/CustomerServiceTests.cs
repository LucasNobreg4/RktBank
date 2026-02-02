using Application.Constants;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces;
using Moq;

namespace TestBank.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMemoryCacheService> _cacheServiceMock;
        private readonly CustomerService _sut;

        public CustomerServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _cacheServiceMock = new Mock<IMemoryCacheService>();

            _sut = new CustomerService(
                _customerRepositoryMock.Object,
                _userRepositoryMock.Object,
                _cacheServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateCustomerAsync_WhenUserNotFound_ReturnsFailure()
        {
            var dto = new CreateCustomerDto { Document = "12345678900" };
            var userId = "user-123";

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _sut.CreateCustomerAsync(dto, userId);

            Assert.False(result.Success);
            Assert.Equal("Usuário não encontrado.", result.Message);
        }

        [Fact]
        public async Task CreateCustomerAsync_WhenDocumentAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                Document = "12345678900",
                Type = "Individual",
                FullName = "Test User",
                Email = "test@test.com",
                BirthDate = DateTime.Now.AddYears(-30)
            };
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var initialized = true;
            _cacheServiceMock
                .Setup(x => x.TryGetValue<bool>(CostumersKeys.Initialized, out initialized))
                .Returns(true);

            var existingCustomer = new CustomerDto { Document = dto.Document };
            _cacheServiceMock
                .Setup(x => x.TryGetValue(CostumersKeys.ByDocument(dto.Document), out existingCustomer))
                .Returns(true);

            // Act
            var result = await _sut.CreateCustomerAsync(dto, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cliente com este documento já existe.", result.Message);
        }

        [Fact]
        public async Task CreateCustomerAsync_WhenValidData_ReturnsSuccess()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                Document = "12345678900",
                Type = "Individual",
                FullName = "John Doe",
                Email = "john@test.com",
                BirthDate = DateTime.Now.AddYears(-30),
                PhoneNumber = "11999999999",
                Address = "Rua Teste, 123",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01000-000"
            };
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId };

            var createdCustomer = new Customer
            {
                Id = "customer-123",
                Document = dto.Document,
                Type = CustomerType.Individual,
                FullName = dto.FullName,
                Email = dto.Email,
                BirthDate = dto.BirthDate,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var initialized = true;
            _cacheServiceMock
                .Setup(x => x.TryGetValue<bool>(CostumersKeys.Initialized, out initialized))
                .Returns(true);

            CustomerDto? nullCustomer = null;
            _cacheServiceMock
                .Setup(x => x.TryGetValue(CostumersKeys.ByDocument(dto.Document), out nullCustomer))
                .Returns(false);

            _customerRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Customer>()))
                .ReturnsAsync(createdCustomer);

            // Act
            var result = await _sut.CreateCustomerAsync(dto, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cliente criado com sucesso.", result.Message);
            Assert.NotNull(result.Customer);
            Assert.Equal(dto.Document, result.Customer.Document);

            _cacheServiceMock.Verify(x => x.SetPermanent(
                CostumersKeys.ById(createdCustomer.Id),
                It.IsAny<CustomerDto>()), Times.Once);

            _cacheServiceMock.Verify(x => x.Remove(CostumersKeys.All), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_WhenInvalidCustomerType_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                Document = "12345678900",
                Type = "InvalidType",
                FullName = "Test User",
                Email = "test@test.com",
                BirthDate = DateTime.Now.AddYears(-30)
            };
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var initialized = true;
            _cacheServiceMock
                .Setup(x => x.TryGetValue<bool>(CostumersKeys.Initialized, out initialized))
                .Returns(true);

            // Act
            var result = await _sut.CreateCustomerAsync(dto, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Tipo de cliente inválido. Use 'Individual' ou 'Business'.", result.Message);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WhenCustomerExistsInCache_ReturnsCustomer()
        {
            // Arrange
            var customerId = "customer-123";
            var cachedCustomer = new CustomerDto
            {
                Id = customerId,
                Document = "12345678900",
                FullName = "John Doe",
                Email = "john@test.com"
            };

            var initialized = true;
            _cacheServiceMock
                .Setup(x => x.TryGetValue<bool>(CostumersKeys.Initialized, out initialized))
                .Returns(true);

            _cacheServiceMock
                .Setup(x => x.TryGetValue(CostumersKeys.ById(customerId), out cachedCustomer))
                .Returns(true);

            // Act
            var result = await _sut.GetCustomerByIdAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
            Assert.Equal("12345678900", result.Document);

            _customerRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WhenCustomerNotFound_ReturnsNull()
        {
            // Arrange
            var customerId = "non-existent";

            var initialized = true;
            _cacheServiceMock
                .Setup(x => x.TryGetValue<bool>(CostumersKeys.Initialized, out initialized))
                .Returns(true);

            CustomerDto? nullCustomer = null;
            _cacheServiceMock
                .Setup(x => x.TryGetValue(CostumersKeys.ById(customerId), out nullCustomer))
                .Returns(false);

            // Act
            var result = await _sut.GetCustomerByIdAsync(customerId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenCacheInitialized_ReturnsFromCache()
        {
            // Arrange
            var cachedCustomers = new List<CustomerDto>
            {
                new CustomerDto { Id = "1", FullName = "Customer 1" },
                new CustomerDto { Id = "2", FullName = "Customer 2" }
            };

            var initialized = true;
            _cacheServiceMock
                .Setup(x => x.TryGetValue<bool>(CostumersKeys.Initialized, out initialized))
                .Returns(true);

            _cacheServiceMock
                .Setup(x => x.TryGetValue(CostumersKeys.All, out cachedCustomers))
                .Returns(true);

            // Act
            var result = await _sut.GetAllCustomersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Customer 1", result[0].FullName);

            _customerRepositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomerAsync_WhenCustomerNotFound_ReturnsFailure()
        {
            // Arrange
            var customerId = "non-existent";
            var dto = new UpdateCustomerDto { FullName = "Updated Name" };

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _sut.UpdateCustomerAsync(customerId, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cliente não encontrado.", result.Message);
        }

        [Fact]
        public async Task UpdateCustomerAsync_WhenValidData_UpdatesSuccessfully()
        {
            // Arrange
            var customerId = "customer-123";
            var dto = new UpdateCustomerDto
            {
                FullName = "Updated Name",
                Email = "updated@test.com",
                PhoneNumber = "11988888888",
                Address = "New Address",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01000-000"
            };

            var existingCustomer = new Customer
            {
                Id = customerId,
                Document = "12345678900",
                Type = CustomerType.Individual,
                FullName = "Old Name",
                Email = "old@test.com",
                CreatedByUserId = "user-123",
                CreatedAt = DateTime.UtcNow
            };

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _customerRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.UpdateCustomerAsync(customerId, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cliente atualizado com sucesso.", result.Message);
            Assert.Equal("Updated Name", result.Customer!.FullName);
            Assert.Equal("updated@test.com", result.Customer.Email);

            _cacheServiceMock.Verify(x => x.SetPermanent(
                CostumersKeys.ById(customerId),
                It.IsAny<CustomerDto>()), Times.Once);

            _cacheServiceMock.Verify(x => x.Remove(CostumersKeys.All), Times.Once);
        }

        [Fact]
        public async Task DeactivateCustomerAsync_WhenCustomerExists_DeactivatesSuccessfully()
        {
            // Arrange
            var customerId = "customer-123";
            var cachedCustomer = new CustomerDto
            {
                Id = customerId,
                Document = "12345678900"
            };

            _customerRepositoryMock
                .Setup(x => x.DeleteAsync(customerId))
                .ReturnsAsync(true);

            _cacheServiceMock
                .Setup(x => x.TryGetValue(CostumersKeys.ById(customerId), out cachedCustomer))
                .Returns(true);

            // Act
            var result = await _sut.DeactivateCustomerAsync(customerId);

            // Assert
            Assert.True(result);

            _cacheServiceMock.Verify(x => x.Remove(CostumersKeys.ById(customerId)), Times.Once);
            _cacheServiceMock.Verify(x => x.Remove(CostumersKeys.ByDocument("12345678900")), Times.Once);
            _cacheServiceMock.Verify(x => x.Remove(CostumersKeys.All), Times.Once);
        }

        [Fact]
        public async Task DeactivateCustomerAsync_WhenCustomerNotFound_ReturnsFalse()
        {
            // Arrange
            var customerId = "non-existent";

            _customerRepositoryMock
                .Setup(x => x.DeleteAsync(customerId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeactivateCustomerAsync(customerId);

            // Assert
            Assert.False(result);

            _cacheServiceMock.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
        }
    }
}
