using Microsoft.EntityFrameworkCore;
using EcommerceAI.Models;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;

namespace EcommerceAI.Repositories.Implementations;

public class UserActivityRepository : IUserActivityRepository
{
    private readonly AppDbContext _context;

    public UserActivityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserActivity>> GetRecentByUserAsync(Guid userId, int days = 90)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.UserActivities
            .Where(a => a.UserId == userId && a.CreatedAt >= cutoff)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task RecordActivityAsync(UserActivity activity)
    {
        if (activity.ActivityType == "view")
        {
            var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
            var exists = await _context.UserActivities.AnyAsync(a => 
                a.UserId == activity.UserId && 
                a.ProductId == activity.ProductId && 
                a.ActivityType == "view" && 
                a.CreatedAt >= tenMinutesAgo);
            
            if (exists) return;
        }

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync();
    }
}
