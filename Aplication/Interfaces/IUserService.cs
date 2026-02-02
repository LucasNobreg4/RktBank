using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByEmailAsync(string email);
    }
}
