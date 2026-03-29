using EcommerceAI.Contracts.DTOs.Cart;

namespace EcommerceAI.Services.Interfaces;

public interface ICartService
{
    Task<CartResponseDto?> GetByUserIdAsync(Guid userId);
    Task<CartResponseDto> AddItemAsync(Guid userId, AddToCartRequestDto request);
    Task<CartResponseDto?> UpdateItemAsync(Guid userId, Guid productId, int quantity);
    Task<CartResponseDto?> RemoveItemAsync(Guid userId, Guid productId);
    Task<CartResponseDto?> RemoveCartItemAsync(Guid userId, Guid cartItemId);
}
