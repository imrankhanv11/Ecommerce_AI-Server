using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class UserActivityService : IUserActivityService
{
    private readonly IUserActivityRepository _activityRepository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(
        IUserActivityRepository activityRepository,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<UserActivityService> logger)
    {
        _activityRepository = activityRepository;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        // Use 30s timeout as requested
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task RecordActivityAsync(Guid userId, Guid productId, string activityType, int score)
    {
        var activity = new UserActivity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            ActivityType = activityType,
            Score = score,
            CreatedAt = DateTime.UtcNow
        };
        await _activityRepository.RecordActivityAsync(activity);
    }

    public async Task<string> GetActivitySummaryAsync(Guid userId)
    {
        var activities = await _activityRepository.GetRecentByUserAsync(userId, days: 90);
        if (!activities.Any()) return "You haven't started your shopping journey yet. Explore our collection to see personalized insights!";

        var activityList = string.Join(", ", activities.Select(a => $"{a.ActivityType} of product {a.ProductId}"));
        
        try
        {
            var prompt = $"The user has the following shopping activity: {activityList}. " +
                         "Address the user directly in the 2nd person (e.g., 'You seem to love...'). " +
                         "Give a short 3-line friendly summary of their shopping behavior and what they might like. " +
                         "Keep it casual and positive.";

            var model = _configuration["AI:OllamaModel"] ?? "llama3.2:1b";
            var baseUrl = _configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434";
            
            var requestBody = new
            {
                model = model,
                prompt = prompt,
                stream = false
            };

            var url = $"{baseUrl.TrimEnd('/')}/api/generate";
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            
            if (!response.IsSuccessStatusCode) return "Your shopping profile is growing! Keep exploring.";

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("response").GetString() ?? "You have a great eye for quality!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get AI activity summary from Ollama");
            return "You're a savvy shopper with a great eye for our collection!";
        }
    }
}
