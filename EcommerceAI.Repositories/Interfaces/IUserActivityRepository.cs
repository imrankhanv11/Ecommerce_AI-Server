using EcommerceAI.Models.Entities;

namespace EcommerceAI.Repositories.Interfaces;

public interface IUserActivityRepository
{
    Task<List<UserActivity>> GetRecentByUserAsync(Guid userId, int days = 90);
    Task RecordActivityAsync(UserActivity activity);
}
