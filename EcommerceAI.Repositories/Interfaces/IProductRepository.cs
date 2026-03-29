using EcommerceAI.Models.Entities;
using EcommerceAI.Contracts.DTOs.Chatbot;

namespace EcommerceAI.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<(List<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId, decimal? minPrice, decimal? maxPrice,
        List<string>? tags, string? searchTerm, int page, int pageSize);
    Task<List<Product>> GetByIdsAsync(List<Guid> ids);
    Task<List<Product>> GetByCategoryAsync(Guid categoryId);
    Task<List<Product>> GetTopSellingAsync(int limit);
    Task<List<Product>> GetTopByStockAsync(int limit);
    Task<List<Product>> GetByKeywordsAsync(string keyword, int limit);
    Task<List<Product>> GetByCategoryAndKeywordsAsync(string categoryName, string keyword, int limit);
    Task<List<Product>> FilterByChatbotAsync(ChatbotFilterDto filters);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}
