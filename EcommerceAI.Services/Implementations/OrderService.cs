using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Order;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserActivityRepository _activityRepository;

    private static readonly string[] ValidStatuses =
        { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserActivityRepository activityRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _activityRepository = activityRepository;
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request)
    {
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product {item.ProductId} not found.");

            if (product.Stock < item.Quantity)
                throw new ArgumentException($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}");

            // Snapshot the price
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += product.Price * item.Quantity;

            // Reduce stock
            product.Stock -= item.Quantity;
            await _productRepository.UpdateAsync(product);

            // Record purchase activity for recommendation engine
            await _activityRepository.RecordActivityAsync(new UserActivity
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ProductId = product.Id,
                ActivityType = "purchase",
                Score = 5
            });
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Status = "Pending",
            TotalAmount = totalAmount,
            Items = orderItems
        };

        var created = await _orderRepository.CreateAsync(order);
        var result = await _orderRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null || order.UserId != userId) return null;
        return MapToDto(order);
    }

    public async Task<PagedResultDto<OrderResponseDto>> GetByUserIdAsync(
        Guid userId, int page = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _orderRepository.GetByUserIdAsync(userId, page, pageSize);
        return new PagedResultDto<OrderResponseDto>
        {
            Data = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            Limit = pageSize
        };
    }

    public async Task<OrderResponseDto?> UpdateStatusAsync(Guid id, string status)
    {
        if (!ValidStatuses.Contains(status))
            throw new ArgumentException($"Invalid status. Valid statuses: {string.Join(", ", ValidStatuses)}");

        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;

        order.Status = status;
        await _orderRepository.UpdateAsync(order);

        var updated = await _orderRepository.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task<OrderResponseDto?> CancelOrderAsync(Guid id, Guid userId)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null || order.UserId != userId) return null;

        if (order.Status != "Pending")
            throw new InvalidOperationException("Only Pending orders can be cancelled.");

        order.Status = "Cancelled";
        await _orderRepository.UpdateAsync(order);

        return MapToDto(order);
    }

    private static OrderResponseDto MapToDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                CategoryName = i.Product?.Category?.Name ?? "General",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
