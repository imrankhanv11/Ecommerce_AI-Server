using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.User;

public class UpdateUserRequestDto
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [EmailAddress, MaxLength(255)]
    public string? Email { get; set; }
}
