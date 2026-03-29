using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.User;

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
