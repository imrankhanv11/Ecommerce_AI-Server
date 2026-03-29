namespace EcommerceAI.Contracts.DTOs.Activity;

public class UserActivityRequestDto
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public int Score { get; set; }
}
