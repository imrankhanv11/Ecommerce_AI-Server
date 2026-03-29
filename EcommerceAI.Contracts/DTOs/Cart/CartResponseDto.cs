namespace EcommerceAI.Contracts.DTOs.Cart;

public class CartResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(i => i.Subtotal);
    public DateTime UpdatedAt { get; set; }
}

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
