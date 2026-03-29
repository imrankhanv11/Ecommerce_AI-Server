using EcommerceAI.Models.Entities;

namespace EcommerceAI.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}
