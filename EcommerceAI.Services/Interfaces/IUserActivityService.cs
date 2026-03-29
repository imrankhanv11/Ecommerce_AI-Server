using EcommerceAI.Contracts.DTOs.Activity;

namespace EcommerceAI.Services.Interfaces;

public interface IUserActivityService
{
    Task RecordActivityAsync(Guid userId, Guid productId, string activityType, int score);
    Task<string> GetActivitySummaryAsync(Guid userId);
}
