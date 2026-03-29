namespace EcommerceAI.Contracts.Common;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponseDto<T> SuccessResponse(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponseDto<T> FailResponse(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };
}
