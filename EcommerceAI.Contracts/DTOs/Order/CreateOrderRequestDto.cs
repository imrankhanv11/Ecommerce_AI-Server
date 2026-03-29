using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.Order;

public class CreateOrderRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required, MinLength(1)]
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

public class OrderItemRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required, Range(1, 1000)]
    public int Quantity { get; set; }
}
