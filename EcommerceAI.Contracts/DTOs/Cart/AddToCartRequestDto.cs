using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.Cart;

public class AddToCartRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required, Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
