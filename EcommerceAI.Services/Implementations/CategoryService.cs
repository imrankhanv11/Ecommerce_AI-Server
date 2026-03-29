using EcommerceAI.Contracts.DTOs.Category;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug
        }).ToList();
    }
}
