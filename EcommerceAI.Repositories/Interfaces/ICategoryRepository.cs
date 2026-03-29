using EcommerceAI.Models.Entities;

namespace EcommerceAI.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category?> GetBySlugAsync(string slug);
    Task<HashSet<string>> GetAllSlugsAsync();
}
