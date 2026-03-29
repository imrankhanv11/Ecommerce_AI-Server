using EcommerceAI.Contracts.DTOs.Cart;
using EcommerceAI.Contracts.DTOs.Product;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserActivityRepository _activityRepository;
    private readonly IRecommendationService _recommendationService;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUserActivityRepository activityRepository,
        IRecommendationService recommendationService)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _activityRepository = activityRepository;
        _recommendationService = recommendationService;
    }

    public async Task<CartResponseDto?> GetByUserIdAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        return cart == null ? null : MapToDto(cart);
    }

    public async Task<CartResponseDto> AddItemAsync(Guid userId, AddToCartRequestDto request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        if (product.Stock < request.Quantity)
            throw new ArgumentException($"Insufficient stock for product '{product.Name}'.");

        var cart = await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };
            cart = await _cartRepository.CreateAsync(cart);
        }

        // Check if item already exists in cart
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            await _cartRepository.UpdateItemAsync(existingItem);
        }
        else
        {
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
            await _cartRepository.AddItemAsync(cartItem);
        }

        // Record cart_add activity for recommendation engine
        await _activityRepository.RecordActivityAsync(new UserActivity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = request.ProductId,
            ActivityType = "cart_add",
            Score = 2
        });

        // Reload and return updated cart
        var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
        
        // Fire recommendation generation in background to warm the cache
        _ = Task.Run(async () => {
            try {
                await _recommendationService.GetForUserAsync(userId, new RecommendationRequestDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CategoryName = product.Category?.Name ?? "General"
                });
            } catch { /* Silent fail for background task */ }
        });

        return MapToDto(updatedCart!);
    }

    public async Task<CartResponseDto?> UpdateItemAsync(Guid userId, Guid productId, int quantity)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new KeyNotFoundException($"Product {productId} not found in cart.");

        item.Quantity = quantity;
        await _cartRepository.UpdateItemAsync(item);

        var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
        return MapToDto(updatedCart!);
    }

    public async Task<CartResponseDto?> RemoveItemAsync(Guid userId, Guid productId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return null;

        await _cartRepository.RemoveItemAsync(cart.Id, productId);

        var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
        return MapToDto(updatedCart!);
    }

    public async Task<CartResponseDto?> RemoveCartItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return null;

        // Verify ownership
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null) return null; // Or throw access denied if preferred

        await _cartRepository.RemoveItemByIdAsync(cartItemId);

        var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
        return MapToDto(updatedCart!);
    }

    private static CartResponseDto MapToDto(Cart cart)
    {
        return new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.Items.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                UnitPrice = i.Product?.Price ?? 0,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
