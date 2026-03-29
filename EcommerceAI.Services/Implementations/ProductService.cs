using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Product;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || !product.IsActive) return null;
        return MapToDto(product);
    }

    public async Task<PagedResultDto<ProductResponseDto>> GetFilteredAsync(ProductFilterDto filter)
    {
        var (items, totalCount) = await _productRepository.GetFilteredAsync(
            filter.CategoryId, filter.MinPrice, filter.MaxPrice,
            filter.Tags, filter.SearchTerm, filter.Page, filter.PageSize);

        return new PagedResultDto<ProductResponseDto>
        {
            Data = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            Limit = filter.PageSize
        };
    }

    public async Task<ProductResponseDto> CreateAsync(ProductRequestDto request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            Tags = request.Tags,
            IsActive = request.IsActive
        };

        var created = await _productRepository.CreateAsync(product);
        // Re-load with category included
        var result = await _productRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<ProductResponseDto?> UpdateAsync(Guid id, ProductRequestDto request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        product.Name = request.Name;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.Tags = request.Tags;
        product.IsActive = request.IsActive;

        await _productRepository.UpdateAsync(product);
        var result = await _productRepository.GetByIdAsync(id);
        return MapToDto(result!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return false;

        await _productRepository.DeleteAsync(id);
        return true;
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Tags = product.Tags,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }
}
