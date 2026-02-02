using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Util
{
    public static class CustomerUtil
    {

        public static CustomerDto MapToDto(Customer customer)
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
                CreatedByUserId = customer.CreatedByUserId,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                IsActive = customer.IsActive
            };
        }
    }
}
