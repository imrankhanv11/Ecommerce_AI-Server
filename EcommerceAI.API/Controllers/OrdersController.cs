using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Order;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> Create(
        [FromBody] CreateOrderRequestDto request)
    {
        var order = await _orderService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponseDto<OrderResponseDto>.SuccessResponse(order, "Order created successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> GetById(Guid id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(ApiResponseDto<OrderResponseDto>.FailResponse("User not identified"));

        var userId = Guid.Parse(userIdClaim);
        var order = await _orderService.GetByIdAsync(id, userId);
        
        if (order == null)
            return NotFound(ApiResponseDto<OrderResponseDto>.FailResponse("Order not found or access denied"));
            
        return Ok(ApiResponseDto<OrderResponseDto>.SuccessResponse(order));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<OrderResponseDto>>>> GetByUser(
        Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetByUserIdAsync(userId, page, pageSize);
        return Ok(ApiResponseDto<PagedResultDto<OrderResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<OrderResponseDto>>>> GetMyOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(ApiResponseDto<PagedResultDto<OrderResponseDto>>.FailResponse("User not identified"));

        var userId = Guid.Parse(userIdClaim);
        var result = await _orderService.GetByUserIdAsync(userId, page, pageSize);
        return Ok(ApiResponseDto<PagedResultDto<OrderResponseDto>>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> UpdateStatus(
        Guid id, [FromBody] UpdateOrderStatusDto request)
    {
        var order = await _orderService.UpdateStatusAsync(id, request.Status);
        if (order == null)
            return NotFound(ApiResponseDto<OrderResponseDto>.FailResponse("Order not found"));
        return Ok(ApiResponseDto<OrderResponseDto>.SuccessResponse(order, "Order status updated"));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> Cancel(Guid id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(ApiResponseDto<OrderResponseDto>.FailResponse("User not identified"));

        var userId = Guid.Parse(userIdClaim);
        
        try
        {
            var order = await _orderService.CancelOrderAsync(id, userId);
            if (order == null)
                return NotFound(ApiResponseDto<OrderResponseDto>.FailResponse("Order not found or access denied"));
                
            return Ok(ApiResponseDto<OrderResponseDto>.SuccessResponse(order, "Order cancelled successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<OrderResponseDto>.FailResponse(ex.Message));
        }
    }
}
