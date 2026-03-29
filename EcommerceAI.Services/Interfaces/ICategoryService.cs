using EcommerceAI.Contracts.DTOs.Category;

namespace EcommerceAI.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllAsync();
}
