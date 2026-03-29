using EcommerceAI.Contracts.DTOs.User;

namespace EcommerceAI.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserResponseDto?> GetByIdAsync(Guid id);
    Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserRequestDto request);
    Task<UserResponseDto?> UpdateProfileAsync(Guid id, string firstName, string lastName);
}
