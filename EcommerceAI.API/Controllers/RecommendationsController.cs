using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Product;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpPost("{userId:guid}")]
    public async Task<ActionResult<ApiResponseDto<List<ProductResponseDto>>>> GetForUser(
        Guid userId, [FromBody] RecommendationRequestDto request)
    {
        var recommendations = await _recommendationService.GetForUserAsync(userId, request);
        return Ok(ApiResponseDto<List<ProductResponseDto>>.SuccessResponse(recommendations));
    }

    [HttpGet("{userId:guid}/similar/{productId:guid}")]
    public async Task<ActionResult<ApiResponseDto<List<ProductResponseDto>>>> GetSimilar(
        Guid userId, Guid productId)
    {
        var similar = await _recommendationService.GetSimilarAsync(productId);
        return Ok(ApiResponseDto<List<ProductResponseDto>>.SuccessResponse(similar));
    }
}
