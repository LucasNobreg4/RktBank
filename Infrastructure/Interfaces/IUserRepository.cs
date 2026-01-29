using Domain.Entities;

namespace Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetAll();
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(ApplicationUser user, string password);
        Task<(bool Succeeded, bool IsLockedOut)> ValidatePasswordAsync(ApplicationUser user, string password);
        Task SignOutAsync();
    }
}
