namespace EcommerceAI.Contracts.DTOs.Product;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
