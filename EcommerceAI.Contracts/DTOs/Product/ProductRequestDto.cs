using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.Product;

public class ProductRequestDto
{
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(0, 10_000_000)]
    public decimal Price { get; set; }

    [Required, Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public List<string> Tags { get; set; } = new();

    public bool IsActive { get; set; } = true;
}
