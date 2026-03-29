using System.Text.Json;

namespace EcommerceAI.Models.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Tags stored as JSON string for SQL Server compatibility.
    /// Use the Tags property to get/set the list.
    /// </summary>
    public string TagsJson { get; set; } = "[]";

    public List<string> Tags
    {
        get => JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new();
        set => TagsJson = JsonSerializer.Serialize(value);
    }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();
}
