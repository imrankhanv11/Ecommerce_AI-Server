namespace EcommerceAI.Contracts.DTOs.Chatbot;

public class ChatbotFilterDto
{
    public string? Category { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinPrice { get; set; }
    public List<string> Tags { get; set; } = new();
}
