using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Cart;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> GetByUser(Guid userId)
    {
        var cart = await _cartService.GetByUserIdAsync(userId);
        if (cart == null)
            return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(
                new CartResponseDto { UserId = userId }, "Cart is empty"));
        return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(cart));
    }

    [HttpPost("{userId:guid}/items")]
    public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> AddItem(
        Guid userId, [FromBody] AddToCartRequestDto request)
    {
        var cart = await _cartService.AddItemAsync(userId, request);
        return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(cart, "Item added to cart"));
    }

    [HttpPut("{userId:guid}/items/{productId:guid}")]
    public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> UpdateItem(
        Guid userId, Guid productId, [FromBody] UpdateCartItemDto request)
    {
        var cart = await _cartService.UpdateItemAsync(userId, productId, request.Quantity);
        if (cart == null)
            return NotFound(ApiResponseDto<CartResponseDto>.FailResponse("Cart not found"));
        return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(cart, "Cart item updated"));
    }

    [HttpDelete("{userId:guid}/items/{productId:guid}")]
    public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> RemoveItem(
        Guid userId, Guid productId)
    {
        var cart = await _cartService.RemoveItemAsync(userId, productId);
        if (cart == null)
            return NotFound(ApiResponseDto<CartResponseDto>.FailResponse("Cart not found"));
        return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(cart, "Item removed from cart"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> GetMyCart()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(ApiResponseDto<CartResponseDto>.FailResponse("User not identified"));

        var cart = await _cartService.GetByUserIdAsync(userId);
        if (cart == null)
            return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(
                new CartResponseDto { UserId = userId }, "Cart is empty"));
        return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(cart));
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<CartResponseDto>>> RemoveCartItemById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(ApiResponseDto<CartResponseDto>.FailResponse("User not identified"));

        var cart = await _cartService.RemoveCartItemAsync(userId, id);
        if (cart == null)
            return NotFound(ApiResponseDto<CartResponseDto>.FailResponse("Cart item not found or access denied"));
            
        return Ok(ApiResponseDto<CartResponseDto>.SuccessResponse(cart, "Item removed secure-style"));
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }
}
