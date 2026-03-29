using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Activity;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IUserActivityService _activityService;

    public ActivityController(IUserActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<bool>>> Record([FromBody] UserActivityRequestDto request)
    {
        await _activityService.RecordActivityAsync(request.UserId, request.ProductId, request.ActivityType, request.Score);
        return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Activity logged"));
    }

    [HttpGet("summary/{userId:guid}")]
    public async Task<ActionResult<ApiResponseDto<string>>> GetSummary(Guid userId)
    {
        var summary = await _activityService.GetActivitySummaryAsync(userId);
        return Ok(ApiResponseDto<string>.SuccessResponse(summary));
    }
}
