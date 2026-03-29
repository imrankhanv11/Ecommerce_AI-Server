using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.Order;

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
