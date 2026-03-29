using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Chatbot;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("query")]
    public async Task<ActionResult<ApiResponseDto<ChatbotResponseDto>>> Query(
        [FromBody] ChatbotQueryRequestDto request)
    {
        var userId = GetCurrentUserId();
        // If user is not logged in, we can still process query but without personal context
        // However, the service now expects a userId. 
        // For now, if not logged in, we'll return a fail or use a guest ID.
        // Let's enforce login for the chatbot to keep it simple and secure as requested.
        
        if (userId == Guid.Empty)
            return Unauthorized(ApiResponseDto<ChatbotResponseDto>.FailResponse("Please login to use Aura Assistant"));

        var result = await _chatbotService.ProcessQueryAsync(userId, request);
        return Ok(ApiResponseDto<ChatbotResponseDto>.SuccessResponse(result));
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }
}
