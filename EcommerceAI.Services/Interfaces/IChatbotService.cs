using EcommerceAI.Contracts.DTOs.Chatbot;

namespace EcommerceAI.Services.Interfaces;

public interface IChatbotService
{
    Task<ChatbotResponseDto> ProcessQueryAsync(Guid userId, ChatbotQueryRequestDto request);
}
