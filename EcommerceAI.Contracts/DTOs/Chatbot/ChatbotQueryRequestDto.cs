using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.Chatbot;

public class ChatbotQueryRequestDto
{
    [Required, MinLength(1), MaxLength(300)]
    public string Query { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    
    public List<ChatHistoryItem> History { get; set; } = new();
}

public class ChatHistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
