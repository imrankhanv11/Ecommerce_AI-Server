using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Order;

namespace EcommerceAI.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request);
    Task<OrderResponseDto?> GetByIdAsync(Guid id, Guid userId);
    Task<PagedResultDto<OrderResponseDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<OrderResponseDto?> UpdateStatusAsync(Guid id, string status);
    Task<OrderResponseDto?> CancelOrderAsync(Guid id, Guid userId);
}
