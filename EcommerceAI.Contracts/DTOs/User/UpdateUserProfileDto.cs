using System.ComponentModel.DataAnnotations;

namespace EcommerceAI.Contracts.DTOs.User;

public class UpdateUserProfileDto
{
    [Required, MaxLength(100), MinLength(1)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100), MinLength(1)]
    public string LastName { get; set; } = string.Empty;
}
