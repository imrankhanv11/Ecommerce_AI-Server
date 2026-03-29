namespace EcommerceAI.Models.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
