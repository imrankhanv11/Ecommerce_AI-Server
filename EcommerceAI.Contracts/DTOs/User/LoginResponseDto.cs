namespace EcommerceAI.Contracts.DTOs.User;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserResponseDto User { get; set; } = new();
}
