using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using EcommerceAI.Contracts.DTOs.Product;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class RecommendationService : IRecommendationService
{
    private readonly IProductRepository _productRepository;
    private readonly IUserActivityRepository _activityRepository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RecommendationService> _logger;
    private readonly IMemoryCache _cache;

    private const int SameCategoryBonus = 3;

    public RecommendationService(
        IProductRepository productRepository,
        IUserActivityRepository activityRepository,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<RecommendationService> logger,
        IMemoryCache cache)
    {
        _productRepository = productRepository;
        _activityRepository = activityRepository;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
        
        // 15s timeout for faster responses as requested
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<List<ProductResponseDto>> GetForUserAsync(Guid userId, RecommendationRequestDto request)
    {
        _logger.LogInformation("Recommendation Request: {Product} from {Category} (ID: {CatId})", 
            request.ProductName, request.CategoryName, request.CategoryId);
            
        string cacheKey = $"strict_recs_{userId}_{request.ProductId}";

        if (_cache.TryGetValue(cacheKey, out List<ProductResponseDto>? cachedRecs))
            return cachedRecs ?? new();

        try
        {
            var prompt = $@"Customer just added '{request.ProductName}' from '{request.CategoryName}' to their cart.

Suggest 5 products a customer would ALSO buy with this item.
   
STRICT RULES:
- ONLY suggest products from '{request.CategoryName}' category or categories that are very closely related.
- Example: Toys & Games -> only suggest toys, games, kids books, kids clothing. NEVER suggest groceries or electronics.
- Example: Electronics -> only suggest phone cases, chargers, cables, headphones. NEVER suggest food or toys.
- Example: Grocery & Food -> only suggest other food items, kitchen tools, cooking items. NEVER suggest electronics.
- Be very specific — suggest exact product types not general categories
   
Reply in JSON only, no markdown, no extra text:
[
  {{ ""productType"": ""exact item"", ""category"": ""{request.CategoryName}"" }},
  {{ ""productType"": ""exact item"", ""category"": ""{request.CategoryName}"" }},
  {{ ""productType"": ""exact item"", ""category"": ""{request.CategoryName}"" }},
  {{ ""productType"": ""exact item"", ""category"": ""{request.CategoryName}"" }},
  {{ ""productType"": ""exact item"", ""category"": ""{request.CategoryName}"" }}
]";

            var model = _configuration["AI:OllamaModel"] ?? "llama3.2:1b";
            var baseUrl = _configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434";
            
            var requestBody = new { model = model, prompt = prompt, stream = false, format = "json" };
            var url = $"{baseUrl.TrimEnd('/')}/api/generate";
            
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            if (!response.IsSuccessStatusCode)
                return await GetFallbackRecommendations(request.CategoryName, request.CategoryId, request.Limit);

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            var rawResponse = doc.RootElement.GetProperty("response").GetString() ?? "";
            
            var cleanedJson = rawResponse.Replace("```json", "").Replace("```", "").Trim();
            var suggestedTypes = JsonSerializer.Deserialize<List<JsonElement>>(cleanedJson)!;

            var results = new List<ProductResponseDto>();
            
            // PRIORITY 1: Match suggested items in SAME category
            foreach (var typeObj in suggestedTypes)
            {
                var typeName = typeObj.TryGetProperty("productType", out var pt) ? pt.GetString() : null;
                if (string.IsNullOrEmpty(typeName)) continue;

                var matched = await _productRepository.GetByCategoryAndKeywordsAsync(request.CategoryName, typeName, 2);
                results.AddRange(matched.Select(MapToDto));
            }

            var finalRecs = results.DistinctBy(p => p.Id)
                                .Where(p => p.Id != request.ProductId)
                                .Take(request.Limit).ToList();
            
            // PRIORITY 2: Fill remaining slots with random items from SAME category
            if (finalRecs.Count < request.Limit)
            {
                var sameCategory = await _productRepository.GetByCategoryAsync(request.CategoryId);
                var extras = sameCategory
                    .Where(p => p.Id != request.ProductId && !finalRecs.Any(r => r.Id == p.Id))
                    .OrderBy(x => Guid.NewGuid())
                    .Take(request.Limit - finalRecs.Count)
                    .Select(MapToDto);
                finalRecs.AddRange(extras);
            }

            // PRIORITY 3: Absolute fallback (Newest in Category)
            if (finalRecs.Count < 2)
            {
                var newestResult = await _productRepository.GetFilteredAsync(request.CategoryId, null, null, null, null, 1, 6);
                var extras = newestResult.Items
                    .Where(p => p.Id != request.ProductId && !finalRecs.Any(r => r.Id == p.Id))
                    .Select(MapToDto);
                finalRecs.AddRange(extras);
            }

            finalRecs = finalRecs.DistinctBy(p => p.Id).Take(request.Limit).ToList();
            _cache.Set(cacheKey, finalRecs, TimeSpan.FromMinutes(5));
            return finalRecs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recommendation pipeline error");
            return await GetFallbackRecommendations(request.CategoryName, request.CategoryId, request.Limit);
        }
    }

    private async Task<List<ProductResponseDto>> GetFallbackRecommendations(string categoryName, Guid categoryId, int limit)
    {
        var newest = await _productRepository.GetFilteredAsync(categoryId, null, null, null, null, 1, limit);
        return newest.Items
            .OrderByDescending(p => p.CreatedAt)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<List<ProductResponseDto>> GetSimilarAsync(Guid productId, int limit = 8)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return new();

        var candidates = await _productRepository.GetByCategoryAsync(product.CategoryId);

        return candidates
            .Where(p => p.Id != productId && p.IsActive)
            .Select(p => new
            {
                Product = p,
                Score = SameCategoryBonus + p.Tags.Intersect(product.Tags).Count()
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => MapToDto(x.Product))
            .ToList();
    }

    private static ProductResponseDto MapToDto(Models.Entities.Product product)
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
