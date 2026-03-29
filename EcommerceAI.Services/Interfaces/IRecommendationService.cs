using EcommerceAI.Contracts.DTOs.Product;

namespace EcommerceAI.Services.Interfaces;

public interface IRecommendationService
{
    Task<List<ProductResponseDto>> GetForUserAsync(Guid userId, RecommendationRequestDto request);
    Task<List<ProductResponseDto>> GetSimilarAsync(Guid productId, int limit = 8);
}
