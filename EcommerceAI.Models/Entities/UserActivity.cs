namespace EcommerceAI.Models.Entities;

public class UserActivity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }

    /// <summary>
    /// Activity type: "view", "cart_add", or "purchase"
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>
    /// Pre-computed score: view=1, cart_add=2, purchase=5
    /// </summary>
    public int Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
