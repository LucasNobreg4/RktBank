using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RktBank.Controllers;
using System.Security.Claims;

namespace TestBank.Controllers
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly CustomerController _sut;

        public CustomerControllerTests()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _sut = new CustomerController(_customerServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Email, "test@test.com")
            }, "mock"));

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateCustomer_WhenSuccess_ReturnsOk()
        {
            var dto = new CreateCustomerDto
            {
                Document = "12345678900",
                Type = "Individual",
                FullName = "John Doe",
                Email = "john@test.com",
                BirthDate = DateTime.Now.AddYears(-30)
            };

            var response = new CustomerResponseDto
            {
                Success = true,
                Message = "Cliente criado com sucesso.",
                Customer = new CustomerDto { Id = "customer-123", Document = dto.Document }
            };

            _customerServiceMock
                .Setup(x => x.CreateCustomerAsync(dto, "user-123"))
                .ReturnsAsync(response);

            var result = await _sut.CreateCustomer(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CustomerResponseDto>(okResult.Value);
            Assert.True(resultValue.Success);
            Assert.Equal(dto.Document, resultValue.Customer!.Document);
        }

        [Fact]
        public async Task CreateCustomer_WhenFails_ReturnsBadRequest()
        {
            var dto = new CreateCustomerDto { Document = "12345678900" };

            var response = new CustomerResponseDto
            {
                Success = false,
                Message = "Cliente com este documento já existe."
            };

            _customerServiceMock
                .Setup(x => x.CreateCustomerAsync(dto, "user-123"))
                .ReturnsAsync(response);

            var result = await _sut.CreateCustomer(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsOkWithList()
        {
            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = "1", FullName = "Customer 1" },
                new CustomerDto { Id = "2", FullName = "Customer 2" }
            };

            _customerServiceMock
                .Setup(x => x.GetAllCustomersAsync())
                .ReturnsAsync(customers);

            var result = await _sut.GetAllCustomers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsAssignableFrom<List<CustomerDto>>(okResult.Value);
            Assert.Equal(2, resultValue.Count);
        }

        [Fact]
        public async Task GetCustomerById_WhenExists_ReturnsOk()
        {
            var customerId = "customer-123";
            var customer = new CustomerDto
            {
                Id = customerId,
                FullName = "John Doe"
            };

            _customerServiceMock
                .Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(customer);

            var result = await _sut.GetCustomerById(customerId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CustomerDto>(okResult.Value);
            Assert.Equal(customerId, resultValue.Id);
        }

        [Fact]
        public async Task GetCustomerById_WhenNotFound_ReturnsNotFound()
        {
            var customerId = "non-existent";

            _customerServiceMock
                .Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync((CustomerDto?)null);

            var result = await _sut.GetCustomerById(customerId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCustomerByDocument_WhenExists_ReturnsOk()
        {
            var document = "12345678900";
            var customer = new CustomerDto
            {
                Id = "customer-123",
                Document = document,
                FullName = "John Doe"
            };

            _customerServiceMock
                .Setup(x => x.GetCustomerByDocumentAsync(document))
                .ReturnsAsync(customer);

            var result = await _sut.GetCustomerByDocument(document);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CustomerDto>(okResult.Value);
            Assert.Equal(document, resultValue.Document);
        }

        [Fact]
        public async Task GetMyCustomers_ReturnsOkWithList()
        {
            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = "1", CreatedByUserId = "user-123" },
                new CustomerDto { Id = "2", CreatedByUserId = "user-123" }
            };

            _customerServiceMock
                .Setup(x => x.GetCustomersByUserAsync("user-123"))
                .ReturnsAsync(customers);

            var result = await _sut.GetMyCustomers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsAssignableFrom<List<CustomerDto>>(okResult.Value);
            Assert.Equal(2, resultValue.Count);
        }

        [Fact]
        public async Task UpdateCustomer_WhenSuccess_ReturnsOk()
        {
            var customerId = "customer-123";
            var dto = new UpdateCustomerDto { FullName = "Updated Name" };

            var response = new CustomerResponseDto
            {
                Success = true,
                Message = "Cliente atualizado com sucesso.",
                Customer = new CustomerDto { Id = customerId, FullName = "Updated Name" }
            };

            _customerServiceMock
                .Setup(x => x.UpdateCustomerAsync(customerId, dto))
                .ReturnsAsync(response);

            var result = await _sut.UpdateCustomer(customerId, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CustomerResponseDto>(okResult.Value);
            Assert.True(resultValue.Success);
        }

        [Fact]
        public async Task UpdateCustomer_WhenFails_ReturnsBadRequest()
        {
            var customerId = "customer-123";
            var dto = new UpdateCustomerDto { FullName = "Updated Name" };

            var response = new CustomerResponseDto
            {
                Success = false,
                Message = "Cliente não encontrado."
            };

            _customerServiceMock
                .Setup(x => x.UpdateCustomerAsync(customerId, dto))
                .ReturnsAsync(response);

            var result = await _sut.UpdateCustomer(customerId, dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeactivateCustomer_WhenSuccess_ReturnsNoContent()
        {
            var customerId = "customer-123";

            _customerServiceMock
                .Setup(x => x.DeactivateCustomerAsync(customerId))
                .ReturnsAsync(true);

            var result = await _sut.DeactivateCustomer(customerId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeactivateCustomer_WhenNotFound_ReturnsNotFound()
        {
            var customerId = "non-existent";

            _customerServiceMock
                .Setup(x => x.DeactivateCustomerAsync(customerId))
                .ReturnsAsync(false);

            var result = await _sut.DeactivateCustomer(customerId);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
