using EcommerceAI.Contracts.DTOs.AI;
using EcommerceAI.Contracts.DTOs.Chatbot;

namespace EcommerceAI.Services.Interfaces;

public interface IChatbotService
{
    Task<ChatbotResponseDto> ProcessQueryAsync(Guid userId, ChatbotQueryRequestDto request);
    Task<string> GenerateCancellationMessageAsync(List<CancellationItemDto> items, string customerName);
    Task<string> GetProductInsightAsync(string productName);
}
