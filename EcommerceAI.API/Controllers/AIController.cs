using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IChatbotService _chatbotService;
    private readonly Microsoft.Extensions.Logging.ILogger<AIController> _logger;

    public AIController(IConfiguration configuration, IChatbotService chatbotService, Microsoft.Extensions.Logging.ILogger<AIController> logger)
    {
        _configuration = configuration;
        _chatbotService = chatbotService;
        _logger = logger;
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        var provider = _configuration["AI:Provider"] ?? "Ollama";
        var isOllama = provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase);
        
        var response = new
        {
            provider = provider,
            model = isOllama ? (_configuration["AI:OllamaModel"] ?? "llama3.2:1b") : "Gemini-1.5-Flash",
            ollamaUrl = isOllama ? (_configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434") : null,
            status = isOllama ? "configured" : (!string.IsNullOrEmpty(_configuration["AI:GeminiApiKey"]) ? "configured" : "API key missing")
        };

        return Ok(response);
    }

    [HttpPost("cancellation-message")]
    public async Task<IActionResult> GetCancellationMessage([FromBody] EcommerceAI.Contracts.DTOs.AI.CancellationMessageRequestDto request)
    {
        var customerName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Valued Customer";
        
        var message = await _chatbotService.GenerateCancellationMessageAsync(
            request.Items, customerName);
            
        return Ok(new EcommerceAI.Contracts.DTOs.AI.CancellationMessageResponseDto { Message = message });
    }

    [HttpGet("product-insight")]
    public async Task<IActionResult> GetProductInsight([FromQuery] string productName)
    {
        if (string.IsNullOrEmpty(productName)) return BadRequest("Product name is required");
        _logger.LogInformation("Received request for product insight: {ProductName}", productName);
        var insight = await _chatbotService.GetProductInsightAsync(productName);
        _logger.LogInformation("Returning insight for {ProductName}: {InsightText}", productName, insight);
        return Ok(new { insight = insight });
    }
}
