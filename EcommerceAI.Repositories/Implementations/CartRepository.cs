using Microsoft.EntityFrameworkCore;
using EcommerceAI.Models;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;

namespace EcommerceAI.Repositories.Implementations;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> CreateAsync(Cart cart)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart> UpdateAsync(Cart cart)
    {
        cart.UpdatedAt = DateTime.UtcNow;
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task AddItemAsync(CartItem item)
    {
        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(CartItem item)
    {
        _context.CartItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Guid cartId, Guid productId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveItemByIdAsync(Guid cartItemId)
    {
        var item = await _context.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
