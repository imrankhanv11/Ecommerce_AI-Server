using EcommerceAI.Contracts.DTOs.Product;

namespace EcommerceAI.Contracts.DTOs.Chatbot;

public class ChatbotResponseDto
{
    public ChatbotFilterDto ExtractedFilters { get; set; } = new();
    public List<ProductResponseDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public string? WarningMessage { get; set; }
    public string? AIResponse { get; set; }
}
