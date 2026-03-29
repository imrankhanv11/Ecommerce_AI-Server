using EcommerceAI.Models.Entities;

namespace EcommerceAI.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId);
    Task<Cart> CreateAsync(Cart cart);
    Task<Cart> UpdateAsync(Cart cart);
    Task AddItemAsync(CartItem item);
    Task UpdateItemAsync(CartItem item);
    Task RemoveItemAsync(Guid cartId, Guid productId);
    Task RemoveItemByIdAsync(Guid cartItemId);
}
