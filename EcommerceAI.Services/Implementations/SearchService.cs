using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class SearchService : ISearchService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SearchService> _logger;

    public SearchService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SearchService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        // 30s timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return new();

        try
        {
            var prompt = $"User searched for [{keyword}] on an ecommerce site. " +
                         "Suggest 3 related short search terms they might also mean. " +
                         "Reply in JSON only: [\"term1\", \"term2\", \"term3\"]";

            var model = _configuration["AI:OllamaModel"] ?? "llama3.2:1b";
            var baseUrl = _configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434";
            
            var requestBody = new
            {
                model = model,
                prompt = prompt,
                stream = false,
                format = "json"
            };

            var url = $"{baseUrl.TrimEnd('/')}/api/generate";
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            
            if (!response.IsSuccessStatusCode) return new();

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            var aiJsonResponse = doc.RootElement.GetProperty("response").GetString();

            if (string.IsNullOrEmpty(aiJsonResponse)) return new();

            return JsonSerializer.Deserialize<List<string>>(aiJsonResponse) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get AI search suggestions from Ollama");
            return new();
        }
    }
}
