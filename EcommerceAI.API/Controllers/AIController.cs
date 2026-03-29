using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AIController(IConfiguration configuration)
    {
        _configuration = configuration;
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
}
