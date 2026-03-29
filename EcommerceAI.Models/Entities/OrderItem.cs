namespace EcommerceAI.Models.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    /// <summary>
    /// Snapshot of the product price at the time of order.
    /// Does NOT track future price changes.
    /// </summary>
    public decimal UnitPrice { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
