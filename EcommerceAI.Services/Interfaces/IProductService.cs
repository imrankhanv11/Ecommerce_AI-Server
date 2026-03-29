using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Product;

namespace EcommerceAI.Services.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto?> GetByIdAsync(Guid id);
    Task<PagedResultDto<ProductResponseDto>> GetFilteredAsync(ProductFilterDto filter);
    Task<ProductResponseDto> CreateAsync(ProductRequestDto request);
    Task<ProductResponseDto?> UpdateAsync(Guid id, ProductRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
