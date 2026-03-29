using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.Product;

public class RecommendationRequestDto
{
    public Guid? ProductId { get; set; }
    
    [Required]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    public string CategoryName { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    
    public int Limit { get; set; } = 10;
}
