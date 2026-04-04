namespace EcommerceAI.Contracts.DTOs.AI;

public class CancellationMessageRequestDto
{
    public List<CancellationItemDto> Items { get; set; } = new();
}

public class CancellationItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class CancellationMessageResponseDto
{
    public string Message { get; set; } = string.Empty;
}
